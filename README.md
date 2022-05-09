# Project Ferrite (Telegram Server)

 [![.NET](https://github.com/aykutalparslan/ferrite/actions/workflows/dotnet.yml/badge.svg)](https://github.com/aykutalparslan/ferrite/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/aykutalparslan/ferrite/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/aykutalparslan/ferrite/actions/workflows/codeql-analysis.yml) [![DevSkim](https://github.com/aykutalparslan/ferrite/actions/workflows/devskim.yml/badge.svg)](https://github.com/aykutalparslan/ferrite/actions/workflows/devskim.yml)

Project Ferrite is an implementation of the Telegram Server API in C#. 

## What works?

The project is currently in a very early stage of development. The following are the features that are implemented and working so far:
- All MTProto transports are implemented (only Abridged and Intermediate transports are tested)
- Websockets and Obfuscation
- Creation of an Auth Key
- MTProto Encryption/Decryption (AES-IGE, AES-CTR, RSA with custom padding etc.)
- TL Serialization/Deserialization

## License

Project Ferrite is licensed under GNU AGPL-3.0

### Special Thanks

![Rider logo](https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.svg)

[Telegram-Server]: <https://github.com/aykutalparslan/Telegram-Server/>
