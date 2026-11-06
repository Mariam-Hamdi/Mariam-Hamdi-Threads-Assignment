#
Monitor
--->    
The lock compiles to Monitor.Enter and Monitor.Exit and ,onitor give us
capability and control that are not available in lock 
#

=========
#
Lock 
-->
it locks threads not variables and it coordinates the 
thread inside one process which ask the same object
#
==========
#
difference them
--->
The lock is a simple and safe shortcut for basic locking,
while Monitor is the underlying powerful engine that gives
you advanced control like TryEnter, Wait, and Pulse.
#
==============

#
Monitor.TryEnter: When a thread should not wait forever if the code is busy.
It tries to enter, and if locked, it can do something else or wait 
for a specific time
#

=============
#
Monitor.Wait and Monitor.Pulse: When threads need to coordinate and
talk to each other (like a producer and consumer). One thread waits 
(Wait) until a condition is met, and another thread wakes it up (Pulse)
when ready
#