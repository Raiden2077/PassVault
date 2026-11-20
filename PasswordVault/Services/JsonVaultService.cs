using PasswordVault.Models;
using System;
using System.Collections.Generic;

using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordVault.Services
{
    public class JsonVaultService : IVaultService
    {
        public string Password { private get; set; } = string.Empty;
        public string FilePath { get; set; } = "passwords.enc";
        public List<PasswordEntry> Load()
        {
            if (!File.Exists(FilePath)) return [];

            List<PasswordEntry> entries = [];
            FileEncryptor.DecryptFile(FilePath, entries, Password);
            return entries;
        }
        public void Save(IEnumerable<PasswordEntry> entries)
        {
            string tempPath = $"{FilePath}.tmp";
            FileEncryptor.EncryptFile(entries, tempPath, Password);
            File.Move(tempPath, FilePath, overwrite: true);
        }

    }

    public static class FileEncryptor
    {
        private const int SaltSize = 16;      // 128 бит
        private const int NonceSize = 12;     // 96 бит (стандарт для GCM)
        private const int TagSize = 16;       // 128 бит
        private const int KeySize = 32;       // 256 бит (AES-256)
        private const int Iterations = 600_000;

        public static void EncryptFile(IEnumerable<PasswordEntry> entries, string destinationPath, string masterPassword)
        {
            byte[] plainBytes = JsonSerializer.SerializeToUtf8Bytes(entries, new JsonSerializerOptions { WriteIndented = true });

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] tag = new byte[TagSize];
            byte[] cipherBytes = new byte[plainBytes.Length];

            byte[] key = DeriveKey(masterPassword, salt);

            try
            {
                using var aes = new AesGcm(key, TagSize);
                aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

                using var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                fs.Write(salt);
                fs.Write(nonce);
                fs.Write(tag);
                fs.Write(cipherBytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
                CryptographicOperations.ZeroMemory(plainBytes);
            }
        }

        public static void DecryptFile(string encryptedPath, List<PasswordEntry> entries, string masterPassword)
        {
            byte[] fileData = File.ReadAllBytes(encryptedPath);
            int headerSize = SaltSize + NonceSize + TagSize;
            if (fileData.Length < headerSize)
                throw new InvalidDataException("Файл поврежден или имеет неверный формат.");
            ReadOnlySpan<byte> span = fileData;
            ReadOnlySpan<byte> salt = span.Slice(0, SaltSize);
            ReadOnlySpan<byte> nonce = span.Slice(SaltSize, NonceSize);
            ReadOnlySpan<byte> tag = span.Slice(SaltSize + NonceSize, TagSize);
            ReadOnlySpan<byte> cipherBytes = span.Slice(headerSize);

            byte[] key = DeriveKey(masterPassword, salt);
            byte[] plainBytes = new byte[cipherBytes.Length];

            try
            {
                using var aes = new AesGcm(key, TagSize);
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

                entries.AddRange(JsonSerializer.Deserialize<List<PasswordEntry>>(plainBytes) ?? []);
            }
            catch (AuthenticationTagMismatchException)
            {
                throw new CryptographicException("Неверный мастер-ключ или файл был поврежден.");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
                CryptographicOperations.ZeroMemory(plainBytes);
            }
        }

        private static byte[] DeriveKey(string password, ReadOnlySpan<byte> salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: KeySize);
        }
    }
}
