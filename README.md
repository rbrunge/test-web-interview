# Interview

## 1. hardcoded values - introduce const for each

Anvender v�rdier fra settings .json til at erstatte hardcoded v�rdier i koden.


## 2. private GetEntry - make static

Anvender `method injection` til at g�re metoden `GetEntry` statisk ved at flytte `ContentstackService` til en parameter i metoden.`


## 3. public GetEntry - make async

Anvender `async` og `await` til at g�re metoden `GetEntry` asynkron. Anvender WhenAny til at vente p� den f�rste task der er f�rdig. Hvis det er TimeoutTask der er f�rdig, returneres en fejlmeddelelse.


## 4. MemoryCache - extract to base class (like BaseServiceLogger)

Min forst�else er at udvide den eksisterende `BaseServiceLogger` med en metode til at cache svar fra service, hvis data 
ikke allerede er cached (ReadThrough-caching).


## 5. outgoing calls - limit to 5 concurrent

Anvender `SemaphoreSlim` til at begr�nse antallet af samtidige kald til 5. Dette g�res ved at kalde `WaitAsync` p� `SemaphoreSlim`-objektet.


## 6. Contentstack - add IncludeReference to outgoing call

Fra https://www.contentstack.com/docs/developers/sdks/content-delivery-sdk/dot-net/reference:

```
IncludeReference
Add a constraint that requires a particular reference key details.
```

Et eksempel er:

* Content type: customer_onboarding_modal_module
* Entry uid: blt5fd48487060a9925
* Reference key: reference


## 7. Contentstack - introduce GetEntries method (with or without pagination?)

L�ser det som skip/take funktionalitet. Anvender Contentstacks `Skip` og `Limit` metoder til at implementere dette og metoden `Find`


## 8. Contentstack - support live preview

