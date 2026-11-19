using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace PasswordVault.Services
{
    public class CryptoPasswordGeneratorService : IPasswordGeneratorService
    {
        public string GeneratePassword(int length, bool useUpper, bool useDigits, bool useSymbols)
        {
             var password = new StringBuilder(length);

            string characterAlphabet = useUpper
                ? Alphabets.CharacterUpperAlphabet + Alphabets.CharacterLowerAlphabet
                : Alphabets.CharacterLowerAlphabet;
            string numericAlphabet = useDigits
                ? Alphabets.NumericAlphabet
                : "";
            string symbolAlphabet = useSymbols
                ? Alphabets.SymbolAlphabet
                : "";
            string alphabet = characterAlphabet + numericAlphabet + symbolAlphabet;

            for (int i = 0; i < length; i++)
            {
                password.Append(GetRandomCharacter(alphabet));
            }

            return password.ToString();
        }

        public double EvaluateStrength(string password)
        {
            int poolSize = password.Any(char.IsLower) ? 26 : 0;
            poolSize = password.Any(char.IsUpper) ? poolSize + 26 : poolSize;
            poolSize = password.Any(char.IsDigit) ? poolSize + 10 : poolSize;
            poolSize = password.Any(c => !char.IsLetterOrDigit(c)) ? poolSize + 32 : poolSize;

            if (string.IsNullOrEmpty(password)) return 0;
            double strengthValue = password.Length * Math.Log2(poolSize);

            return Math.Min(120, strengthValue);

        }
        public static char GetRandomCharacter(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

    }
    public static class Alphabets
    {
        public const string NumericAlphabet = "0123456789";
        public const string CharacterUpperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string CharacterLowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        public const string SymbolAlphabet = "~!@#$%^&*()_+{}|:\"?><;[]\\;',./";
    }
}
