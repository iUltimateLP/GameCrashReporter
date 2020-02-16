# GameCrashReporter
👾 Easy-to-use crash reporter and receiver for your game. 👾

Since a lot of game engines don't really open up their crash reporting logic, I wrote my own. Initially tested and used on Unreal Engine 4,
this reporter consists out of two simple parts, the **GameCrashReporter** and the **GameCrashReceiver**.

### GameCrashReporter
This is what the player will see. Called by your game in the event of a crash, it will allow the user to enter a description of what he did
which lead to the crash. The reporter will zip up the crash data and send it over to the receiver.

### GameCrashReceiver
The receiver is a simple HTTP listener server which receives the crashes, decompresses them and stores them in the local filesystem.

### Compatibility
I developed this for Unreal Engine 4, so the reporter won't work out of the box for other engines, but it does provide a simple starting point for
your logic. 

The **CrashReporter** class has three functions: `LoadCrashData()`, `CompressCrashData()` and `SendCrashData()`, which load all crash files into
`FileInfo` holders, compress them into a zip archive and sends them to the remote endpoint.

The **CrashReceiver** class implements a `AsyncHandleHttpRequest()` which handles incoming requests and a `StoreCrash()` function which
implements the decompression and storing of the data.
