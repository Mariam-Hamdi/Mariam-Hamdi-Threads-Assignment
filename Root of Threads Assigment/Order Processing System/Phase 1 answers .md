#
Why are the sync and async times almost the same when orders are awaited one after another?
--->
when we use await inside a foreach loop ,every task waits for the previous one to complete so the total time 
adds up, almost identical to the synchronous version if we execute sequential.
#
=================
#
Is the thread ID before an await always the same as after it? Why? Use your own log lines as evidence.
--->
it is not guarntieed the same thread will return 
and resueme excution after await , this depends on
 sychronizationContext and The thread dpool 
#
===================
#
Task.Sleep()
 --->
 it blocks the thread .The  thread waits and does no anything else .

 Task.Delay()
 --->
 using with it await , it waits callerthread and back it pool for time delay ends
#
==================

#
Why should a repository method return Task<T> instead of T?
---->
we use Task<T> instead of T in Repository methods to support Asynchronous programming,
since database operations is I/O bound return Task<T> allows us to use await,
which prevents thread blocking,frees up the thread pool to handle other requests, improves application scalability.
#
=============
#
What is the difference between File.ReadAllText and File.ReadAllTextAsync? Does the async
version occupy a thread while the disk is reading?
--->
ReadAllText :: 
--> it reads the file syncronously, and blocks the current thread until reading file completely

ReadAllTextAsync ::
--> it reads file asyncronously, it not block the current thread,it realeses and 
back to thread pool to handle other requests, once I/O bound operation complete , the execution resueme 
#