namespace src.api.methods;

public record class Message
{
    public static string ProjectAlreadyExists(string projectName) =>
        $"Project '{projectName}' already exists.";
    public static string ProjectNotFound(string projectName) =>
        $"Project '{projectName}' was not found.";
    public static string EnvAlreadyExists(string projectName, string envName) =>
        $"Environment '{envName}' already exists in the project '{projectName}'.";
    public static string EnvNotFound(string projectName, string envName) =>
        $"Environment '{envName}' was not found in the project '{projectName}'.";
    public static string KeysAlreadyExist(string matchingKeys) =>
        $"Input resource keys '{matchingKeys}' already exist.";
    public static string KeyNotFound(string projectName, string envName, int id) =>
        $"Rsource key ID '{id}' was not found in the environment '{envName}' of the project '{projectName}'.";
    public static string KeysNotFound(string projectName, string envName) =>
        $"Rsource keys ware not found in the environment '{envName}' of the project '{projectName}'.";
    public static string KeyDuplicates(string duplicates) =>
        $"Duplicated input translation key(s) '{duplicates}'.";
    public static string KeysNotMatch(string differentKeys) =>
        $"Input translation key(s) '{differentKeys}' do not match the resource keys.";
    public static string LanguageAlreadyExists(string languageCode) =>
        $"Language code '{languageCode}' already exists.";    
    public static string LanguageNotFound(string languageCode) =>
        $"Language code '{languageCode}' was not found.";
    public static string LanguageNotFound(int id) =>
        $"Language with ID '{id}' was not found.";
    public static string TranslationsAlreadyExist(string languageCode) =>
        $"Tranlations with the language code '{languageCode}' already exist.";
    public static string TranslationNotFound(string envName, string languageCode) =>
        $"Environment '{envName}' with the language code '{languageCode}' was not found.";
}    
