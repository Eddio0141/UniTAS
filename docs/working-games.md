| Game            | Version | Game boots | TAS Runs | Notes                                                                                                                                                                            |
|:--------------- |:------- |:----------:|:--------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [It Steals]     | Latest  |     ✔️     |    ✔️    | Works as expected. Save file remains after soft restart as it's not patched                                                                                                      |
| [yurayura tutt] | Latest  |     ✔️     |    ⚠️    | TAS desyncs due to invoke to `YuraYuraModel.PlayFootStepForReal` and `YuraYuraModel.PlayFootStep` being inconsistent between runs. Stack trace doesn't show what is calling this |
| Hunie Pop       | 1.2.0   |     ✔️     |    ✔️    | Works as expected. Save file remains after soft restart as its not patched                                                                                                       | 
| KTaNE           | ?       |     ⚠️     |    ❌    | Throws an exception on launch                                                                                                                                                    |


[It Steals]: https://store.steampowered.com/app/1349060/It_Steals/
[yurayura tutt]: https://cornflowerblue.itch.io/yurayura-tidying-up-the-tilting-tower
