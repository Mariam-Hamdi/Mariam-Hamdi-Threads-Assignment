# Assignment 02 — Answers

Student name:  Mariam Hamdi Mohammed
Group / ID: 3

## What is wrong
ال threads اتنشأوا داخل الloop 
بس ده مش معناه انهم هيتنفذوا بالترتيب لا
سطر الstart معناه اني ب
ask the os schedule them
ف مش بالضرور يروحوا ينفذوا الفانكشن مع بعض 
لو دخل اكثر من thread هيحص racecondition 
كلهم هيعدوا من الشرط في نفس الوقت وهيروحوا بعدلوا عليه مرة واحدة ف هينقص كتير
-

## How you fixed it
نستخدم lock ------>  condition + operations 

####

## concole after fix racecondition ::
Customer-01 booked a table. Left=4
Customer-02 booked a table. Left=3
Customer-03 booked a table. Left=2
Customer-04 booked a table. Left=1
Customer-05 booked a table. Left=0

Booked=5, TablesLeft=0

D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-02-TableBooking\TableBooking\bin\Debug\net9.0\TableBooking.exe (process 9324) exited with code 0 (0x0).
Press any key to close this window . . .
-
