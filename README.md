# Project Ferrite (Telegram Server)

Project Ferrite is an implementation of the Telegram Server API in C#. 

## What works?

The project is still a work in progress. The following are the features that are implemented and working so far:
- All MTProto transports are implemented (only Abridged and Intermediate transports are tested)
- Websockets and Obfuscation
- Creation of an Auth Key
- MTProto Encryption/Decryption (AES-IGE, AES-CTR, RSA with custom padding etc.)
- TL Serialization/Deserialization
- auth, account, users, contacts, photos, upload, help, langpack namespaces have been implemented to some extend
- Saved Messages and Sending messages works with text messages to some extent.

## Debugging the server
- Clone the repository.
```console
git clone https://github.com/aykutalparslan/Ferrite
```
- Start an instance of Redis.
- Start an instance of Cassandra.
- Start an instance of MinIO and update username/password in the Program.cs.
- Start an instance of ElasticSearch and update username/password/fingerprint in the Program.cs.
- Install the latest preview of .NET 7 
- Debug the Ferrite Console Application with your favourite IDE or
```console
dotnet run
```
- Make sure default-private.key and default-public-key are copied to the output directory as those are the keys embedded into the modified client
- Use the [modified Android client](https://github.com/aykutalparslan/Telegram) to test with.

Note: Docker support is planned in the future

## Roadmap

Development is currently focused on getting the Android application to run in a stable state and the planned order of steps to achive that are:
- All settings screens should work on the Android application
- Contacts related features should work as expected
- Phone calls.
- Basic messaging
- Groups and channels

We would need to implement more than half of the API methods to get to this point. Considering as of this writing (29th June 2022) there are almost 350 unimplemented API methods and that we have approximately 20-40 hours of development time per week we should be at this point in about four months.

After the Android application is working as expected the development will focus on the iOS application and then the Destop and Web applications. Basic features are planned to be implemented in the beginning. After all this the next planned steps are:
- There will be a refactor to support memory efficient serialization
- Support for so called API layers will be added
- Optimizations and benchmarks
- Implementation of the missing features

This stage is also estimated to take another four months to reach.

## License

Project Ferrite is licensed under GNU AGPL-3.0

### Special Thanks

<a href="https://jb.gg/OpenSourceSupport"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.svg" width="48"><a/>

[Telegram-Server]: <https://github.com/aykutalparslan/Telegram-Server/>
