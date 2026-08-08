Requisitos funcionais:

-Autenticar usuários (registro/login) e emitir JWT.
-Controlar acesso por papéis: Admin, Instructor, Student.
-Cursos: criar (Admin/Instructor), listar com paginação e filtros (público), detalhar (público), atualizar (Admin/Instructor), remover (Admin).
-Estudantes: criar perfil vinculado ao usuário (Admin), listar (Admin), detalhar/atualizar (Admin ou o próprio), desativar/remover (Admin).
- Matrículas: matricular estudante autenticado em curso, impedir matrícula duplicada, listar matrículas do próprio estudante (ou Admin).
- Validações: título de curso ≥ 3 caracteres; e-mail de estudante válido e único.
- Erros padronizados com status e mensagem clara.
- Documentação: Swagger com esquema Bearer e exemplos; README com como rodar/testar/autenticar.

Requisitos técnicos:
- .NET 8 + ASP.NET Core Web API (Controllers ou Minimal APIs).
- EF Core para persistência (SQLite em dev; SQL Server/Postgres em ambientes maiores).
- ASP.NET Core Identity + JWT Bearer.
- Configurações por variáveis de ambiente/user-secrets; nenhum segredo no repositório.
- Migrations aplicadas e seed mínimo (papéis + usuário admin) de forma idempotente.
- Índices/constraints: e-mail único; unicidade de matrícula (student+course).
- DTOs separados das entidades; paginação e filtros via query string documentados.
- Swagger/OpenAPI com Security Scheme Bearer.
- Repositório GitHub com README de setup/execução.
 HTTPS habilitado e CORS restrito às origens necessárias.

🚀 Como rodar o projeto localmente
1. Clonar o repositório
> git clone https://github.com/seu-usuario/escola-online.git
> cd escola-online

2. Restaurar as dependências
> dotnet restore

3. Aplicar as migrations (criar de dados)
> dotnet ef database update

4. Executar a aplicação
> dotnet run


Tabelas:

1. Courses
   - Id (PK, int)
   - Titulo (string, max 100, required)
   - Descricao (string, required)
   - Categoria (string, max 30, required) → índice
   - CargaHoraria (int, required, > 0) 
   - DataCriacao (datetime)
   - IsDeleted (bool)

2. Students
   - Id (PK, int)
   - NomeCompleto (string, max 100, required)
   - Email (string, unique, required)
   - UserId (string, FK → AspNetUsers, required)
   - DataCadastro (datetime)
   - IsDeleted (bool)

3. Enrollments (tabela de junção explícita)
   - CourseId (PK, FK → Courses)
   - StudentId (PK, FK → Students)
   - Status (enum: Ativo/Cancelado)
   - DataMatricula (datetime)
   - IsDeleted (bool)

Regras de negócio:
- Um aluno não pode se matricular duas vezes no mesmo curso (PK composta)
- Email do aluno deve ser único
- Carga horária deve ser > 0 e em segundos


## 🔐 Autenticação (JWT)

A API utiliza **ASP.NET Core Identity** + **JWT Bearer** para autenticação.

### Roles disponíveis
- `Admin`
- `Instructor`
- `Student`

### Endpoints de Autenticação

#### 1. Registrar novo usuário
- **Método:** `POST`
- **Rota:** `/user/cadastrar`
- **Autenticação:** Não requer
- **Body (JSON):**
```json
{
  "email": "aluno@email.com",
  "password": "Senha@123",
  "nomeCompleto": "João da Silva",
  "role": "Student"
}
- ** Resposta: 201 **
```json
{
   "message": "Usuário registrado com sucesso"
}

#### 2. Login
- **Método:** `POST`
- **Rota:** `/user/login`
- **Autenticação:** Não requer
- **Body (JSON):**
```json
{
  "email": "aluno@email.com",
  "password": "Senha@123",
}
- ** Resposta: 200 **
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2026-08-08T18:00:00Z",
  "refreshToken": "/user/token/refresh"
}


#### 3. Refresh Token
- **Método:** `POST`
- **Rota:** `/user/token/refresh`
- **Autenticação:** Não requer
- **Body (JSON):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
}

- ** Resposta: 200 **
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2026-08-08T18:00:00Z",
  "refreshToken": "/user/token/refresh"
}