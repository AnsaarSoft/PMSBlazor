using System.Security.Cryptography;
using PMSModels.Models;

namespace PasswordManagement.Services
{
    public interface IPasswordGenerator
    {
        string Generate(MstSetting settings);
        string Describe(MstSetting settings);
    }

    public sealed class PasswordGenerator : IPasswordGenerator
    {
        private const string SymbolCharacters = "!#@$*^%";
        private const int MaximumSymbolCount = 2;

        public string Generate(MstSetting settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var groups = GetCharacterGroups(settings);
            if (groups.Count == 0)
                throw new InvalidOperationException("Select at least one character type in password settings.");

            if (settings.PasswordLength < 6 || settings.PasswordLength > 20)
                throw new InvalidOperationException("Set a valid password length before generating a password.");

            if (settings.PasswordLength < groups.Count)
                throw new InvalidOperationException("The password length is too short for the selected character types.");

            var nonSymbolCharacters = GetNonSymbolCharacters(settings);
            if (settings.UseSymbols && nonSymbolCharacters.Length == 0 && settings.PasswordLength > MaximumSymbolCount)
                throw new InvalidOperationException("Select at least one non-symbol character type when symbols are enabled.");

            var password = new List<char>(settings.PasswordLength);
            foreach (var group in groups)
                password.Add(group[RandomNumberGenerator.GetInt32(group.Length)]);

            var allCharacters = string.Concat(groups);
            var symbolCount = password.Count(character => SymbolCharacters.Contains(character));

            while (password.Count < settings.PasswordLength)
            {
                var availableCharacters = symbolCount < MaximumSymbolCount
                    ? allCharacters
                    : nonSymbolCharacters;

                var character = availableCharacters[RandomNumberGenerator.GetInt32(availableCharacters.Length)];
                password.Add(character);

                if (SymbolCharacters.Contains(character))
                    symbolCount++;
            }

            for (var index = password.Count - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (password[index], password[swapIndex]) = (password[swapIndex], password[index]);
            }

            return new string(password.ToArray());
        }

        public string Describe(MstSetting settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var characterTypes = new List<string>();
            if (settings.UseUppercase)
                characterTypes.Add("uppercase");
            if (settings.UseLowercase)
                characterTypes.Add("lowercase");
            if (settings.UseNumbers)
                characterTypes.Add("numbers");
            if (settings.UseSymbols)
                characterTypes.Add("symbols");

            var types = characterTypes.Count == 0
                ? "no character types selected"
                : string.Join(" · ", characterTypes);

            return $"{settings.PasswordLength} characters · {types}";
        }

        private static List<string> GetCharacterGroups(MstSetting settings)
        {
            var groups = new List<string>();

            if (settings.UseLowercase)
                groups.Add("abcdefghijklmnopqrstuvwxyz");
            if (settings.UseUppercase)
                groups.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            if (settings.UseNumbers)
                groups.Add("0123456789");
            if (settings.UseSymbols)
                groups.Add(SymbolCharacters);

            return groups;
        }

        private static string GetNonSymbolCharacters(MstSetting settings)
        {
            var characters = string.Empty;

            if (settings.UseLowercase)
                characters += "abcdefghijklmnopqrstuvwxyz";
            if (settings.UseUppercase)
                characters += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (settings.UseNumbers)
                characters += "0123456789";

            return characters;
        }
    }
}
