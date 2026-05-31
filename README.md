<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0a0a0a,50:1a3a5c,100:00aaff&height=200&section=header&text=Fish-Net%20Demo%201&fontSize=52&fontColor=ffffff&fontAlignY=38&desc=Multiplayer%20Coin%20Collector%20%E2%80%94%20Fish-Net%20Networking%20v4&descAlignY=58&descSize=18&animation=fadeIn" width="100%"/>

<br/>

![Unity](https://img.shields.io/badge/Unity%206-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![FishNet](https://img.shields.io/badge/Fish--Net-v4-00aaff?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows%20PC-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![Status](https://img.shields.io/badge/Status-Complete-brightgreen?style=for-the-badge)

</div>

---

## 📖 About This Project

A hands-on **Fish-Net v4** multiplayer demo built to explore the core features of [Fish-Net (FishNet)](https://fish-networking.gitbook.io/docs/) — one of the most capable and developer-friendly open-source Unity networking solutions available, built by **FirstGearGames**.

The demo is a simple **multiplayer coin collection game**: players connect, move around the world collecting spawned coins, and compete on a live scoreboard — all synchronized across the network in real time.

> *First one to collect the most coins wins. Simple game. Serious networking.*

---

## 🎮 Gameplay

```
Players connect (Host / Client)
         ↓
CoinSpawner spawns coins server-side
         ↓
Players move around collecting coins
         ↓
Coin triggers pickup → score updates on server
         ↓
Scoreboard syncs updated scores to all clients
         ↓
Game continues until session ends
```

---

## 🏗️ Project Architecture

```
Assets/
└── Scripts/
    ├── NetworkStarter.cs     # Host / client connection bootstrapper
    ├── PlayerController.cs   # Networked player movement & input
    ├── CoinSpawner.cs        # Server-authoritative coin spawning
    ├── Coin.cs               # Coin pickup logic + ownership handling
    ├── Scoreboard.cs         # Synced score tracking & UI display
    └── GameManager.cs        # Core game loop and session management
```

### How Each Script Uses Fish-Net

| Script | Fish-Net Feature Used |
|---|---|
| `NetworkStarter` | `NetworkManager` — starts host/client, manages lifecycle |
| `PlayerController` | `NetworkBehaviour` + `[IsOwner]` — owner-only input, synced movement |
| `CoinSpawner` | `[ServerRpc]` / server-only spawn logic — authoritative spawning |
| `Coin` | `OnStartServer` / `OnStopServer` — server-side pickup validation |
| `Scoreboard` | `SyncVar` / `SyncDictionary` — auto-synced score state |
| `GameManager` | `[ObserversRpc]` — broadcast game events to all clients |

---

## 🐟 Fish-Net vs Netcode for GameObjects

This demo was built to compare Fish-Net against Unity's built-in NGO. Key differences explored:

| Feature | Fish-Net v4 | NGO (Netcode for GameObjects) |
|---|---|---|
| Ownership model | Flexible — server or client owned | Host-authoritative by default |
| `SyncVar` | Built-in, zero boilerplate | `NetworkVariable<T>` |
| RPC types | `[ServerRpc]` `[ObserversRpc]` `[TargetRpc]` | `[ServerRpc]` `[ClientRpc]` |
| IL code generation | ✅ Automatic via Mono.Cecil weaving | ✅ Source generators |
| Performance | High — designed for scale | Moderate |
| Open source | ✅ Free, community-driven | ✅ Unity official |
| Documentation | Excellent | Good |

---

## 🛠️ Tech Stack

| Area | Technology |
|---|---|
| Engine | Unity 6 (6000.3.10f1) |
| Language | C# 9.0 |
| Networking | Fish-Net v4 (`com.firstgeargames.fishnet`) |
| JSON | Newtonsoft.Json |
| Navigation | Unity AI Navigation (NavMesh) |
| UI | TextMesh Pro |
| IDE | JetBrains Rider |
| Platform | Windows Standalone |

---

## 🚀 Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download)
- Unity **6000.3.10f1**
- Two instances of the build (or two PCs on the same network)

### Setup

```bash
# 1. Clone the repo
git clone https://github.com/Tonoychan/Fishnet-Demo-1.git

# 2. Open Unity Hub → Add project → Select the cloned folder

# 3. Let packages resolve (Fish-Net will auto-import)

# 4. Open the main scene and hit Play ▶
```

### Running Multiplayer

```
Instance 1 → Click "Start Host"
Instance 2 → Enter host IP → Click "Join"

Both players will spawn and coins will begin appearing.
```

---

## 🗺️ Roadmap

- [x] Unity 6 project setup with Fish-Net v4
- [x] `NetworkStarter` — host / client connection flow
- [x] Networked player spawning with ownership
- [x] `PlayerController` — owner-only input with synced movement
- [x] `CoinSpawner` — server-authoritative coin spawning
- [x] `Coin` — server-validated pickup with despawn
- [x] `Scoreboard` — real-time synced score display
- [x] `GameManager` — session and game loop management

---

## 👨‍💻 Author

**Tonoy Chakraborty**

---

<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:00aaff,50:1a3a5c,100:0a0a0a&height=120&section=footer" width="100%"/>

*Built with Unity 6 · Networked with Fish-Net v4 · Coins were harmed in the making* 🪙

</div>
