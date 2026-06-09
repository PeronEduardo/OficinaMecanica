\# Sistema de Gestão para Oficina Mecânica



Projeto desenvolvido para a disciplina de Programação III — 2º Bimestre.



\## Tecnologias Utilizadas

\- C# / .NET MAUI (.NET 10)

\- MySQL + Dapper (banco principal)

\- MongoDB (logs de auditoria)

\- SQLite (configurações locais)

\- QuestPDF (relatórios PDF)

\- BCrypt.Net (hash de senhas)



\## Como Executar

1\. Abrir o MySQL Workbench e executar o script `oficina\_mecanica.sql`

2\. Abrir o projeto no Visual Studio 2022

3\. Verificar a connection string em `Infrastructure/MySqlConnectionFactory.cs`

4\. Pressionar F5 para executar



\## Usuários para Teste



| Login    | Senha    | Perfil    |

|----------|----------|-----------|

| admin    | senha123 | Admin     |

| carlos   | senha123 | Mecânico  |

| roberto  | senha123 | Mecânico  |

| ana      | senha123 | Atendente |

| fernanda | senha123 | Atendente |



\## Funcionalidades Implementadas

\- Login com autenticação BCrypt

\- CRUD completo de Clientes, Veículos, Serviços e Peças

\- Ordens de Serviço com relacionamento N:N

\- Relatórios em PDF (QuestPDF)

\- Gráficos nativos MAUI

\- Exportação e importação de dados em JSON

\- Logs de auditoria no MongoDB

\- Exportação de logs em XML

\- Configurações locais no SQLite



\## Arquitetura

MVC + Service Layer + DAO + Interfaces (SOLID)

