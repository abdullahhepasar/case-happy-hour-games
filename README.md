# README #

This README would normally document whatever steps are necessary to get your application up and running.

# Hi, I'm Abdullah! 👋

I created this project for Happy Hour Games and I hope everything goes well.
## Features

- Multiplayer System (with PUN2)
- Server Management (with Playfab)
- Web Service and Storage (with Microsoft Azure)
- User Login System (with Playfab)
- Matchmaking (with PUN2 and Playfab matchmaking ticket)
- Character Creation
- Resource Management
- Select Characters and Move Them (Click to Move in Multiplayer Mode)
- Local Data Storage
- C# Code Patterns
- Multi-Resolution User Interface

Included Plugins:
- [Playfab](https://github.com/PlayFab/UnitySDK)
- [PUN 2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922)
- [GPUInstancer](https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-117566)
- [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
- [Easy Performant Outline](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/easy-performant-outline-2d-3d-urp-hdrp-and-built-in-renderer-157187)
- [Feel](https://assetstore.unity.com/packages/tools/particles-effects/feel-183370)
- [Odin Inspector and Serializer](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)
- [UI Animator](https://assetstore.unity.com/)
## Installation

- This project was created in unity v2022.3.10f1.
- This project requires the Azure Function App Project. You can find it [here](https://github.com/abdullahhepasar/azure-playfab-happyhourgames).


For Azure Web Service Configuration: (.net6)
- Create a free Azure account.
- Set up a function app in Azure.
- Create a storage account in Azure.
- Provide azure function app configurations in the link. (You can find it in Constants.cs)

For Photon (PUN2) Configuration:
- Create a Photon account.
- Create a new App -> Manage -> Custom Server Provider for Playfab

For Playfab Configuration:
- Create a Playfab account.
- Automation->Functions->Register Cloud Script Function (Functian Name: GameLaunchCounter, Trigger type: HTTP)
- Provide integration with Photon.

## Documentation

Brief Explanations:

Implemented Mechanics:
- Technically, the game starts in AppStartActivityManager.cs. Init operations of classes are performed.
- Playfab login operation is performed. (PlayfabManager.cs -> Login())
- After users enter the room in PUN2, it is started with PlayerMultiplayer.cs -> CreateUnits() in its own colors and 3 pieces.
- SelectCharacters.cs is also defined in UIGamePlay prefab and its function selects the characters belonging to the player in the selected area. (The function can be turned on and off with the button). The area is selected by holding GetMouseButtonDown(0) and when it is released, the outline of the selected characters appears.
- Selection with character movements Under PlayerMultiplayer.cs -> ProcessInputs(), the closest Navmesh area to the clicked point is marked with GetMouseButtonDown(1) and the characters are positioned with NavMeshAgent.destination.
- Resources are collected with CharacterController.cs -> Trigger events. It recognizes the Resource type in the current area and is added to the player's PlayerResourceData with the event func. defined in the gathering animation. Here, the recording process to Playfab is triggered when the Resource resource is finished and is also added to the Variable_ResWood(_Project/Resources/Variables) resource located locally.

Multiplayer Features:
- Photon receives the Authentication Token from the PlayfabClientAPIController class. (GetPhotonAuthenticationToken)
- UIMainMenu prefab is started from UIManager.cs (_Project/Resources/UI/UIPrefabs). The CreateMatchmakingTicket() function in UIMainMenu.cs -> PlayfabMultiplayerAPIController.cs is triggered with the Find match button. And the search is started with the "QUEUE" previously defined in Playfab Matchmaking.
- After the match is found, after the players enter the playfab group with LauncherController.cs -> Matchmaking(), the last one that entered starts the room and the others listen for the name of the room that was started with LauncherController.cs -> OnRoomListUpdate PUN2 and connect when the room is ready.

Azure Integration:
- After the Azure account is opened, the Function App is opened via the web. (.net6-isoleted) The necessary variables are added in the "Environment variables". (PLAYFAB_DEV_SECRET_KEY, PLAYFAB_TITLE_ID etc.)
- The opened Azure "Storage Account" and the Function App are connected. "TableGameLaunchCounter" is created from "Tables". PartitionKey: "Playfab", RowKey: "PlayfabID" are defined. And the requested GameLaunch int variable is added.
- Azure Func. In the app, HappyHourGamesPlayfab.cs was connected to CloudStorageAccount and this table was interacted with. (AzureTableRepository.cs)
- "GameLaunchCounter" HTTP connection was added with Automation->Cloud Script->Register function from Playfab and the Function URL was connected with the "GameLaunchCounter" that I created in Azure Func. App.
- From Unity, it is called with PlayfabClientAPIController.cs -> CallExecuteFunction() every time a Playfab session is opened and its value is shown in the bottom left of the UI.

Technical Considerations:
- The local Func. App project was not deployed to the app created via the Azure Function App web. After installing the necessary plugins in VS Code as a different method, the problem was solved in the created app.
- The server in Azure Function App was changed 3 times as a result of some servers being closed. (Current: East US)


## Author

- [@abdullahhepasar](https://github.com/abdullahhepasar)


## License

This project is licensed under the MIT License - see the LICENSE file for details.


[MIT](https://choosealicense.com/licenses/mit/)

