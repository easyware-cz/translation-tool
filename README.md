# Translation tool

## Motivation
Some of the software we made need to have localizations for various languages. Translations of the texts are typically done by an external party so there is a need for a dedicated solution for them to be able to manage those translations.

## General Description
### Overview
Web application that will have a list of projects (software that we made) defined for which it will be possible to define a list of supported localizations (en, en-US, de-DE,...). Each project will also have a dynamic number of environments (PROD, TEST, STAGING,...).

### Users
Users will consist of following roles:
    1. System admin (can create projects)
    2. Project admin (can create localizations, environments and users with Translator role)
    3. Translator (can edit translations and load/upload translations to environments that have been granted to him by the project admin)

### Import/Export
It will be possible to manually import/export translations via RESX format.

### Load/Upload of translations
Each project will have an REST endpoint for load (import) of new resourcekeys and REST endpoint used for pushing the changes to that project (typically a standard user will have that possibility only on the TEST environment).

Load:
    GET /resources?cultureCode={localizationCode}

Push:
    PUT /resources?cultureCode={localizationCode}

For both the exchange format will be a DTO with properties that make sense for that use-case (Key, Value, Description,...).

## Basic Model for storing Translations/Resources
### Resource Key
Project will have a list of "ResourceKeys" which will have:
    
Key:
    something like textual id of the resource (Didyouforgotyourpassword)

Default value:
    typically default text in English (Did you forgot your password?)

Description:
    description of the Key (Text asking user whether he/she forgot the password)

### Resource
Translation of the resource key for specific project and localizaion