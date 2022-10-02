# Project Ferrite (Experimental Telegram Server)

Project Ferrite is an implementation of the Telegram Server API in C# and this repo records it's work-in-progress. Development is focused on implementing must have features before the first release.

## What works?

The following are the features that are implemented and working so far:
- All MTProto transports are implemented (only Abridged and Intermediate transports are tested)
- Websockets and Obfuscation
- Creation of an Auth Key
- MTProto Encryption/Decryption (AES-IGE, AES-CTR, RSA with custom padding etc.)
- TL Serialization/Deserialization
- auth, account, users, contacts, photos, upload, help, langpack namespaces have been implemented to some extend
- Saved Messages and Sending messages works with text messages only.

## Debugging the server
Debugging previously required an infrastructure comprised of Redis, Cassandra, MinIO and ElasticSearch. Currently however Ferrite has a pluggable storage system and local data stores based on RocksDB, FASTER and Lucene are implemented as well as an in-memory cache so we won't need that infrastructure for debugging.
- Clone the repository.
```console
git clone https://github.com/aykutalparslan/Ferrite
```
- Install .NET 7 
- Make sure default-private.key and default-public-key are copied to the output directory as those are the keys embedded into the modified client.
- Make sure Ferrite.Data/LangData is also copied to the output directory.
- Debug the Ferrite Console Application with your favourite IDE or
```console
dotnet run
```
- Telegram protocol requires clients to have the server's public key.
- Use the [modified Android client](https://github.com/aykutalparslan/Telegram) to test with.


## Roadmap

Development is currently focused on getting the Android application to run in a stable state and the planned order of steps to achive that are:
- All settings screens should work on the Android application
- Contacts related features should work as expected
- Phone calls.
- Basic messaging
- Groups and channels

After the Android application is working as expected the development will focus on the iOS application and then the Desktop and Web applications. Basic features are planned to be implemented in the beginning. After all this the next planned steps are:
- There will be a refactor to support memory efficient serialization
- Support for so called API layers will be added
- Optimizations and benchmarks
- Implementation of the missing features


## License

Project Ferrite is licensed under GNU AGPL-3.0

### Special Thanks

<a href="https://jb.gg/OpenSourceSupport"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.svg" width="48"><a/>

[Telegram-Server]: <https://github.com/aykutalparslan/Telegram-Server/>
