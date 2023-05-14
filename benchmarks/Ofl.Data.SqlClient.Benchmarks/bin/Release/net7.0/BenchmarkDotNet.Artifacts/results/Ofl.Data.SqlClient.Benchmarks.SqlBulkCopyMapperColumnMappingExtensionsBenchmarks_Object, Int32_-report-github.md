``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Rows | Offset | Step |      Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |----- |------- |----- |----------:|---------:|---------:|------:|----------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **51.78 ms** | **0.990 ms** | **1.179 ms** |  **1.00** |      **60 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |          |       |           |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** | **1000** |      **0** |    **1** | **128.26 ms** | **2.343 ms** | **1.829 ms** |  **1.00** |     **150 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |          |       |           |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **48.57 ms** | **0.413 ms** | **0.366 ms** |  **1.00** |      **55 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |          |       |           |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** | **1000** |      **0** |    **1** | **118.30 ms** | **2.055 ms** | **3.260 ms** |  **1.00** |     **120 B** |        **1.00** |
