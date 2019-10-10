### Scenario
A, B & C are 3 class libraries targeting netstandard. 

A -> B with PrivateAssets="compile", but A's public surface area has a dependency on B.

C -> A is a package reference that's effectively unuseable.
The [following](https://github.com/nkolev92/NuGet.Tools/blob/master/ApiCreep/C/Program.cs#L10) won't compile.
