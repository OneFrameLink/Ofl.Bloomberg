``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Rows | Offset | Step |     Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |----- |------- |----- |---------:|---------:|---------:|------:|----------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** | **1000** |      **0** |    **1** | **26.21 ms** | **1.044 ms** | **2.997 ms** |  **1.00** |      **19 B** |        **1.00** |
|                        |                  |              |         |      |        |      |          |          |          |       |           |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** | **1000** |      **0** |    **1** | **37.63 ms** | **1.950 ms** | **5.748 ms** |  **1.00** |      **46 B** |        **1.00** |
|                        |                  |              |         |      |        |      |          |          |          |       |           |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** | **1000** |      **0** |    **1** | **21.19 ms** | **0.406 ms** | **0.317 ms** |  **1.00** |      **19 B** |        **1.00** |
|                        |                  |              |         |      |        |      |          |          |          |       |           |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** | **1000** |      **0** |    **1** | **29.72 ms** | **0.573 ms** | **0.508 ms** |  **1.00** |      **43 B** |        **1.00** |
