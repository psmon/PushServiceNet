# Akka.NET 사용가이드    

다음버전으로 필요하면 사용할것

```
<AkkaVersion>1.5.40</AkkaVersion>
<AkkaPersistenceVersion>1.5.37</AkkaPersistenceVersion>
<AkkaMinVersion>1.5.13</AkkaMinVersion>

  <ItemGroup>
    <PackageReference Include="Akka" Version="$(AkkaVersion)" />
    <PackageReference Include="Akka.Remote" Version="$(AkkaVersion)" />
    <PackageReference Include="Akka.Streams" Version="$(AkkaVersion)" />
    <PackageReference Include="Akka.DependencyInjection" Version="$(AkkaVersion)" />
    <PackageReference Include="Akka.Logger.NLog" Version="$(AkkaMinVersion)" />
    <PackageReference Include="Akka.Persistence.RavenDB" Version="$(AkkaPersistenceVersion)" />
    <PackageReference Include="Akka.Persistence.RavenDB.Hosting" Version="$(AkkaPersistenceVersion)" />            
    <PackageReference Include="NLog" Version="5.4.0" />    
  </ItemGroup>
```

