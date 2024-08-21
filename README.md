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

## Author

- [@abdullahhepasar](https://github.com/abdullahhepasar)


## License

This project is licensed under the MIT License - see the LICENSE file for details.


[MIT](https://choosealicense.com/licenses/mit/)

