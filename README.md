 ### FYI
 This repository exists solely so that I can share my current code with different companies that I want to get a job with, since the rest of the repositories are not an actual indicator of my skills at the moment (03/21/2023).
 
 This is code-only (all assets were stripped out) part of unfinished tower defense game.
Despite the fact that I have not optimized the game logic, it still works super-fast, almost without creating allocations.
From interesting architectural solutions: all entities in the game are represented by a single structure — the game logic itself decides how to process an entity based on its EntityType value — this is such a semi-ESC approach to solving the problem of combining different capabilities (components) of entities.
Also, all game logic is completely separated from the presentation logic.


### Preview of the game:
![](https://github.com/sqdrck/UnfinishedTowerDefense/blob/main/Images/output.gif?raw=true)

