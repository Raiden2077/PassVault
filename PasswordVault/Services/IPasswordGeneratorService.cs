using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordVault.Services
{
    public interface IPasswordGeneratorService
    {
        public string GeneratePassword(int length, bool useUpper, bool useDigits, bool useSymbols);
        public double EvaluateStrength(string password);
    }
}
