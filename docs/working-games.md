| Game            | Version |     Game boots     |      TAS Runs      | Notes                                                                                                                                                                            |
|:--------------- |:------- |:------------------:|:------------------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [It Steals]     | Latest  | :heavy_check_mark: | :heavy_check_mark: | Works as expected. Save file remains after soft restart as it's not patched                                                                                                      |
| [yurayura tutt] | Latest  | :heavy_check_mark: |     :warning:      | TAS desyncs due to invoke to `YuraYuraModel.PlayFootStepForReal` and `YuraYuraModel.PlayFootStep` being inconsistent between runs. Stack trace doesn't show what is calling this |
| Hunie Pop       | 1.2.0   |     :warning:      |        :x:         | Hard crash: Unknown caused a Privileged Instruction (0xc0000096) in module Unknown                                                                                                                                                                                 |
| KTaNE           | ?       |      :warning:      |        :x:         | Hard crash: KERNELBASE.dll caused an Access Violation (0xc0000005) in module KERNELBASE.dll


[It Steals]: https://store.steampowered.com/app/1349060/It_Steals/
[yurayura tutt]: https://cornflowerblue.itch.io/yurayura-tidying-up-the-tilting-tower
