# How to manually add a game to the list
- Modify `UniTAS/docs/working-games.json` and add a new entry following the format of the other entries
- If providing the link to the game, make sure the game name is `[Game Name]`, and add the link to the bottom of this file following the format of the other entries
- Use [json-to-markdown](https://tableconvert.com/json-to-markdown) to convert the json to markdown
  - For the `table generator`, select `First row as header` and `Bold first row` and set text align to `Left`
- Replace the table in [Working games](#working-games) with the generated markdown table

# Working tiers
- Perfect
  - Works as UniTAS intends to
- Good
  - No problem TASing, has some unnoticable issues or bugs that doesn't affect the game / TAS
- Desync
  - TAS desyncs, but no major issues
- Broken
  - Game doesn't function as intended, either in TAS playback or normal gameplay
  - This also counts if an exception prevents the game from running
- Crash
  - Game crashes on launch due to UniTAS
  - This is if the game hard crashes

# Working games
| **Game** | **Version** | **Status** | **Notes** |
|---|---|---|---|
| [It Steals] | Latest | Perfect |  |
| [Do It For Me] | Latest | Perfect |  |
| [yurayura tutt] | Latest | Desync | TAS desyncs due to invoke to `YuraYuraModel.PlayFootStepForReal` and `YuraYuraModel.PlayFootStep` being inconsistent between runs. Stack trace doesn't show what is calling this |
| [HuniePop] | 1.2.0 | Broken | Exception on game start |
| [KTaNE] | ? | Broken | Exception on game start |

[It Steals]: https://store.steampowered.com/app/1349060/It_Steals/
[yurayura tutt]: https://cornflowerblue.itch.io/yurayura-tidying-up-the-tilting-tower
[Do It For Me]: https://lixiangames.itch.io/doitforme
[HuniePop]: https://store.steampowered.com/app/339800/HuniePop/
[KTaNE]: https://store.steampowered.com/app/341800/Keep_Talking_and_Nobody_Explodes/
