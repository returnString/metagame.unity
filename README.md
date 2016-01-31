# metagame.unity
[metagame](https://github.com/returnString/metagame) client and example game for the Unity 5 engine.

The client comprises the basic MetagameClient MonoBehaviour and associated types to interact with a running metagame server. The example game makes use of metagame's [sample game data](https://github.com/returnString/metagame/tree/master/sample_game) and the MetagameClient to demonstrate how a Unity game could be structured.

# Demo features
- Authentication - just using the debug platform for now
- Matchmaking example flow
- Applying persistent player changes (currency)

**NB**: This repo *isn't* a good example of best practices for secure online game dev, it's just a quick example of consuming the metagame API with an easy-to-test P2P model. In the real world, your clients wouldn't be able to request the 'grantCurrency' change themselves; a dedicated server would apply it on their behalf.