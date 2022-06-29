# Project Ferrite (Telegram Server)

Project Ferrite is an implementation of the Telegram Server API in C#. 

## What works?

The project is currently in a very early stage of development. The following are the features that are implemented and working so far:
- All MTProto transports are implemented (only Abridged and Intermediate transports are tested)
- Websockets and Obfuscation
- Creation of an Auth Key
- MTProto Encryption/Decryption (AES-IGE, AES-CTR, RSA with custom padding etc.)
- TL Serialization/Deserialization
- auth, account, users, contacts, photos, upload, help, langpack namespaces have been implemented to some extend

## License

Project Ferrite is licensed under GNU AGPL-3.0

### Special Thanks

<a href="https://jb.gg/OpenSourceSupport"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.svg" width="48"><a/>

[Telegram-Server]: <https://github.com/aykutalparslan/Telegram-Server/>
