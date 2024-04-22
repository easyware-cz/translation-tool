namespace src.api.methods;

public record class Message
{
    public static string ProjectAlreadyExists(string projectName) {
        return $"Project '{projectName}' already exists.";
    }
    public static string ProjectNotFound(string projectName) {
        return $"Project '{projectName}' was not found.";
    }
    public static string EnvAlreadyExists(string projectName, string envName) {
        return $"Environment '{envName}' already exists in the project '{projectName}'.";
    }
    public static string EnvNotFound(string projectName, string envName) {
        return $"Environment '{envName}' was not found in the project '{projectName}'.";
    }
    public static string KeysAlreadyExist(string matchingKeys) {
        return $"Input resource keys '{matchingKeys}' already exist.";
    }
    public static string KeyNotFound(string projectName, string envName, int id) {
        return $"Rsource key ID '{id}' was not found in the environment '{envName}' of the project '{projectName}'.";
    }
    public static string KeysNotFound(string projectName, string envName) {
        return $"Rsource keys ware not found in the environment '{envName}' of the project '{projectName}'.";
    }
    public static string KeyDuplicates(string duplicates) {
        return $"Duplicated input translation key(s) '{duplicates}'.";
    }
    public static string KeysNotMatch(string differentKeys) {
        return $"Input translation key(s) '{differentKeys}' do not match the resource keys.";
    }
    public static string LanguageNotFound(string languageCode) {
        return $"Language code '{languageCode}' was not found.";
    }
    public static string TranslationsAlreadyExist(string languageCode) {
        return $"Tranlations with the language code '{languageCode}' already exist.";
    }
    public static string TranslationNotFound(string envName, string languageCode) {
        return $"Environment '{envName}' with the language code '{languageCode}' was not found.";
    }
}    
