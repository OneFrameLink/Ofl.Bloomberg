``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Rows | Offset | Step |      Mean |     Error |    StdDev |    Median | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |----- |------- |----- |----------:|----------:|----------:|----------:|------:|----------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **65.04 ms** |  **1.734 ms** |  **5.029 ms** |  **63.94 ms** |  **1.00** |      **75 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |           |           |           |       |           |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** | **1000** |      **0** |    **1** | **367.30 ms** | **12.734 ms** | **35.073 ms** | **355.03 ms** |  **1.00** |     **600 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |           |           |           |       |           |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **59.16 ms** |  **1.177 ms** |  **1.101 ms** |  **59.01 ms** |  **1.00** |      **67 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |           |           |           |       |           |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** | **1000** |      **0** |    **1** | **335.81 ms** |  **6.686 ms** |  **9.801 ms** | **334.68 ms** |  **1.00** |     **600 B** |        **1.00** |
