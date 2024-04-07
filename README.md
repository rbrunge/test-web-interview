# Case-kommentarer

## Unit tests

Jeg har lavet integrationstests, som kalder Contentstacks API og henter data fra endpoint. Jeg har ikke lavet *rene* unit tests med *mocks* på grund af tid. 
For at teste `ContentstackService` uden afhængigheder, skal den ændres i måden Contentstack-klienten bliver oprettet på.

### ContentstackServiceIntegrationTests
Indeholder test af `ContentstackService`-klassen. Uden mocks.

### ContentstackClientIntegrationTests
Tests lavet for at kender Contentstacks API bedre.


## Kommentarer til svar til TODOs

### 1. hardcoded values - introduce const for each

Anvender værdier fra settings .json til at erstatte hardcoded værdier i koden.


### 2. private GetEntry - make static

Anvender `method injection` til at gøre metoden `GetEntry` statisk ved at flytte `ContentstackService` til en parameter i metoden.`


### 3. public GetEntry - make async

Anvender `async` og `await` til at gøre metoden `GetEntry` asynkron. Anvender WhenAny til at vente på den første task der er færdig. Hvis det er TimeoutTask der er færdig, returneres en fejlmeddelelse.


### 4. MemoryCache - extract to base class (like BaseServiceLogger)

Min forståelse er at udvide den eksisterende `BaseServiceLogger` med en metode til at cache svar fra service, hvis data 
ikke allerede er cached (ReadThrough-caching).


### 5. outgoing calls - limit to 5 concurrent

Anvender `SemaphoreSlim` til at begrænse antallet af samtidige kald til 5. Dette gøres ved at kalde `WaitAsync` på `SemaphoreSlim`-objektet.


### 6. Contentstack - add IncludeReference to outgoing call

Fra https://www.contentstack.com/docs/developers/sdks/content-delivery-sdk/dot-net/reference:

```
IncludeReference
Add a constraint that requires a particular reference key details.
```

Et eksempel er:

* Content type: customer_onboarding_modal_module
* Entry uid: blt5fd48487060a9925
* Reference key: reference


### 7. Contentstack - introduce GetEntries method (with or without pagination?)

Læser det som skip/take funktionalitet. Anvender Contentstacks `Skip` og `Limit` metoder til at implementere dette og metoden `Find`


### 8. Contentstack - support live preview

Contentstacks måde at lave WYSIWYG og have en preview-feature. Har ikke noget kode til denne.