# Assignment 05 — Answers

Student name:  mariam hamdi mohammed
Group / ID: 3

## Failures you observed
 What you changed
             if (!Views.ContainsKey(product)) ---->
if there many thread pass this condition ,they add same key more time and causes Exception the key must unique
#### second problem
if there many threads pass condition "  if (EmailJobs.Count > 0) "
and dequeu more than one and catch  InvalidOperationException because the collection is not thread safe and the business rule is not safe too because the condition is not atomic and the collection is not thread safe
-

## What you changed
    in first problem  -->    Using ConcurrentDictionary 
            in second problem  -->    Using ConcurrentQueue and using lock to make the condition atomic

-

## Collection safety vs business-rule safety
   Collection safety ::  ConcurrentDictionary, ConcurrentQueue
   ل دخلوا كذا thread
   عليهم مش هيحصلCrache ----->thread safe collections 
-
   # business-rule safety::
   مش معنى اني استخدمت Collection safety 
   ان كدا الbusiness-rule safety لا لان ممكن بعد م ظبطت الqueue 
   ييجي في اضافة الlist
   و كذا thread
 ,يضيفوا في نفس الوقت ويحصلها Data Corruption 
 لان الlist
 اصلا not safe 
 عشان كدا استخدمنا الlock
   -
