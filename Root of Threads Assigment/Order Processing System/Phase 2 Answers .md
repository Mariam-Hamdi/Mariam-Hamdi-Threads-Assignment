#
What is the difference between starting a task and awaiting it?
----->
 start task :: ask the os scheduale it,
 but await task :: Non blocking thread ---> when use it with I/O bound 
 operations causes pause to execution,
 current thread frees up the thread pool to handle other requests
 until operation end and return thread to resume execution of the task.
 #
===================

#
What happens if you call .Result or .Wait() on an unfinished task instead of await? Why is it
dangerous? Write a small separate demo method — the only place .Result / .Wait() is allowed 
----->
It blocks the current thread and does nothing else. 
It causes deadlocks (a cycle of waiting where threads cannot continue).
It causes thread pool starvation when many blocked threads put pressure
on the pool to create extra threads. 
And .Result (or .Wait()) wraps the failure in an AggregateException.
..> It is only acceptable in legacy Console 
applications because there is no SynchronizationContext 
(unlike Web APIs or UI applications), so it won't cause a deadlock
#
==========
#
Does every Task create a new thread? 
---->
 No, Tasks are not a threads , they are a promis of a result
 and runs on threads borrowed from pool 
 and these threads reused to handle other requets
#
===================

#
Does Task.Delay() use a thread while waiting?
--->
no, Task.Delay() does not block the thread ,it sets a timer
and the thread is returened to the pool to handle other requests.
#
===================

#
What happens to an exception thrown inside a task that is never awaited?
--->
We do not catch the exception, we won't know which 
task caused the crash, and it becomes an UnobservedTaskException.
#
===================

#
When should you use Task.Run(), and when should you avoid it?
---> 
use it for CPU tasks to keep the main thread free ,
and avoid it for I/O bound operations
, they do not need an extra background threads
#
===================

#
Why can a lock block not contain an await?
--->
The thread that acquires the lock must be the same
thread that releases it , but await
returns the thread to pool , and there is not guarantee
the same thread will resume the execution
#
===================

q6 ___> in answers phase 1 

============
#
Why did the unprotected stock reservation oversell?
--> 
It happens because a race condition , two checked the stock at 
the same time ,Both saw the item was available , so both bought it.
...> how did you prevent it?
 use lockin the stock data and only one request can check and update it at a time .
#

