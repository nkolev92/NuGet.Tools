# What is this? 

This contains an app and a script that repros the concurrency problem with Directory.Move & Directory.CreateDirectory.
The entry point is the script which builds the app. The script bootstraps its own version of the SDK.

# How to use it

4 options are supported. 

`-r` The root directory, where everything will be downloaded. By default this is the script's original directory.
`c` The concurrency level, how many different threads will run. The default is 7. 7 is where the repro happens fairly reliably.
`-o` Whether to use the original implementation, or the implementation with the bool locking (which does not repro).
`v` The version of the SDK used. 2 downloads the 2.2.2xx latest SDK, 3 downloads the 3.0.1xx SDK.

`bash TestApp/test.sh -r /home/nikolev/ConcurencyTest -c 7 -o true -v 3`