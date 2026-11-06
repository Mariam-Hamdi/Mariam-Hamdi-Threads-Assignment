# Assignment 01 — Answers

Student name: mariam Hamdi Mohammed 
Group / ID:  3

## Problems with the current design
in InvoiceExport.cs using foreach 
لو في 500 اوردر عشان نعرف مين فيهم لسه متعلق كلهم هيتسنوا بعض و ده طبعا مشكلة عشان مستخدم ال
foreach
 and execute sequential 
-

## What you changed
استخدمت .select of list & async , await 
كدا الtasks 
هتتنفذ كلها في نفس الفتة الزمنية conccurency progress in the same period 

-

## console output before ::

Starting invoice export...
Customer files: D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\Data
Invoice output: D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices

Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1001.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1002.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1003.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1004.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1005.txt
Export finished in 6.0 s

D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\folderofgithubthreads\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\InvoiceExport.exe (process 13480) exited with code 0 (0x0).
Press any key to close this window . . .

##
## console output after ::

Starting invoice export...
Customer files: D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\Data
Invoice output: D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\invoices

Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\invoices\invoice-1001.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\invoices\invoice-1002.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\invoices\invoice-1003.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\invoices\invoice-1004.txt
Wrote D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\invoices\invoice-1005.txt
Export finished in 6.0 s

D:\Desktop\Desktop2\Simulation .NetAdvanced\session Gittttt\Root of Threads Assigment\bin\Debug\net8.0\Root of Threads Assigment.exe (process 9556) exited with code 0 (0x0).
Press any key to close this window . . . 