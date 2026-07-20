# Microservices Architecture Design
## Chinese Sales API Migration Strategy

**Document Date:** May 2026  
**Status:** Architecture Blueprint | Not for Production Use  
**Audience:** Architecture Team, Tech Leadership, Development Teams

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State & Business Context](#current-state--business-context)
3. [Microservices Decomposition](#microservices-decomposition)
4. [Communication Strategy](#communication-strategy)
5. [Data Strategy & Consistency](#data-strategy--consistency)
6. [Shared Infrastructure](#shared-infrastructure)
7. [Security & Authentication](#security--authentication)
8. [Data Aggregation & Reporting Strategies](#data-aggregation--reporting-strategies)
9. [Migration Roadmap](#migration-roadmap)
10. [Further Considerations](#further-considerations)

---

## Executive Summary

This document outlines the migration of the Chinese Sales API from a monolithic architecture to a **microservices architecture** based on Domain-Driven Design (DDD) principles. The target architecture decomposes the system into **four independent services**, each owning distinct business domains with isolated databases, independent deployment, and clear service boundaries.

### Key Benefits

- **Scalability**: Each service scales independently based on demand
- **Team Autonomy**: Teams can develop, test, and deploy services without coordination overhead
- **Technology Flexibility**: Services can adopt different tech stacks as needed (e.g., Node.js for real-time Lottery Service)
- **Resilience**: Failure in one service doesn't cascade to others; circuit breakers enable graceful degradation
- **Faster Iteration**: Smaller codebases and focused responsibilities enable quicker feature delivery

### Target Services

| Service | Responsibility | Owned Data | Current Source |
|---------|-----------------|-----------|-----------------|
| **Auth Service** | User auth, JWT tokens, role management | Users | AuthController, TokenService |
| **Gift Management Service** | Gifts, categories, donors, gift lifecycle | Gifts, Categories, Donors | GiftController, DonorController, CategoryController |
| **Sales Service** | Basket operations, purchase processing | Baskets, Purchases | BasketController, PurchasesController |
| **Lottery Service** | Draws, winner selection, assignments | Lottery draws, assignments | LotteryController |

---

## Current State & Business Context

### Monolithic Architecture Overview

The existing system is a single ASP.NET Core application deployed as one unit, with:

- **Single Database**: SQL Server via Entity Framework Core (ChineseSaleDbContext)
- **Unified Authentication**: JWT tokens issued by TokenService, validated at controller level via `[Authorize]` attribute
- **Centralized Logging**: Serilog with LoggingHelper utility for structured logs
- **Shared Infrastructure**: Single Redis cache, one CORS policy, unified middleware stack
- **Domain Entities**: User, Gift, Category, Donor, Purchase, Basket, Manager (commented out)
- **Business Logic**: Repository pattern with dedicated services for each domain

### Why Migrate to Microservices?

1. **Scalability Bottleneck**: Gift queries and Lottery draws compete for same database connections
2. **Deployment Coupling**: Small changes in one domain require testing/deployment of entire application
3. **Team Coordination**: Multiple teams working on same codebase creates merge conflicts and coordination overhead
4. **Technology Lock-in**: All services forced to use ASP.NET Core; can't adopt specialized tools for specific domains
5. **Operational Complexity**: One failing component affects entire system; harder to identify root causes

---

## Microservices Decomposition

### Overview: Four Core Services

Each service is responsible for a distinct business capability, owns its data, and exposes clearly defined APIs. Services communicate via REST, gRPC, or asynchronous events.

---

### 1. Auth Service

**Purpose**: Centralized authentication, authorization, and identity management

**Responsibilities**:
- User registration and account management
- Login authentication (email/password validation)
- JWT token generation and distribution
- Token validation and claims extraction
- Role-based access control (User, Admin)
- User profile retrieval

**Owned Data**:
- Users table (Id, Name, Email, Phone, PasswordHash, Role, CreatedAt, UpdatedAt)

**Current Code Mapping**:
- AuthController.cs: Login/Register endpoints
- UserService.cs: Business logic
- UserRepository.cs: Data access
- TokenService.cs: JWT generation
- User.cs model
- JwtSettings.cs configuration

**API Contracts** (REST):
```
POST   /auth/register      → CreateUserDto → ReadUserDto
POST   /auth/login         → LoginDto → { token: string }
POST   /auth/validate      → { token: string } → { valid: bool, claims: object }
GET    /auth/profile       → { id, name, email, phone, role } [Requires JWT]
```

**gRPC Contracts** (for internal calls):
```
service AuthService {
  rpc ValidateToken(ValidateTokenRequest) returns (ValidateTokenResponse);
  rpc GetUserClaims(GetUserClaimsRequest) returns (UserClaimsResponse);
}
```

**Database Schema**:
```sql
CREATE TABLE Users (
  Id INT PRIMARY KEY IDENTITY(1,1),
  Name NVARCHAR(50) NOT NULL,
  Email NVARCHAR(50) NOT NULL UNIQUE,
  Phone NVARCHAR(15) NOT NULL,
  PasswordHash NVARCHAR(70) NOT NULL,
  Role NVARCHAR(20) NOT NULL,  -- 'User' or 'Admin'
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  UpdatedAt DATETIME
);
```

**Dependencies**: None (foundational service)

**Notes**:
- Operates in "hot path" for every API call; must be highly available (99.99% SLA)
- Should implement rate limiting on login/register endpoints to prevent brute force
- Token validation must be fast; consider caching public key for signature verification

---

### 2. Gift Management Service

**Purpose**: Manage gift catalog, categories, and donor information

**Responsibilities**:
- Gift CRUD operations (create, read, update, delete)
- Category management and hierarchies
- Donor profile management and administration
- Gift search and filtering by category
- Inventory tracking and availability checks
- Image asset management (references wwwroot/assets)

**Owned Data**:
- Gifts table (Id, Name, Description, Price, CategoryId, DonorId, ImagePath, WinnerId, CreatedAt, UpdatedAt)
- Categories table (Id, Name, CreatedAt)
- Donors table (Id, Name, Email, Phone, Contribution, CreatedAt)

**Current Code Mapping**:
- GiftController.cs: Gift endpoints
- DonorController.cs: Donor endpoints
- CategoryController.cs: Category endpoints
- GiftService.cs, DonorService.cs, CategoryService.cs: Business logic
- GiftRepository.cs, DonorRepository.cs, CategoryRepository.cs: Data access
- Gift.cs, Donor.cs, Category.cs models

**API Contracts** (REST):
```
GET    /gifts               → PagedResponse<GiftDTO>
GET    /gifts/{id}          → GiftDTO
GET    /gifts/category/{categoryId} → GiftDTO[]
POST   /gifts               → CreateGiftDTO → GiftDTO [Admin Only]
PUT    /gifts/{id}          → UpdateGiftDTO → GiftDTO [Admin Only]
DELETE /gifts/{id}          → GiftDTO [Admin Only]

GET    /donors              → DonorDTO[] [Admin Only]
GET    /donors/{id}         → DonorDTO [Admin Only]
POST   /donors              → CreateDonorDTO → DonorDTO [Admin Only]
PUT    /donors/{id}         → UpdateDonorDTO → DonorDTO [Admin Only]
DELETE /donors/{id}         → DonorDTO [Admin Only]

GET    /categories          → CategoryDTO[]
POST   /categories          → CreateCategoryDTO → CategoryDTO [Admin Only]
```

**gRPC Contracts** (for internal calls):
```
service GiftService {
  rpc GetGift(GetGiftRequest) returns (Gift);
  rpc CheckGiftAvailability(CheckGiftAvailabilityRequest) returns (AvailabilityResponse);
  rpc AssignWinner(AssignWinnerRequest) returns (AssignWinnerResponse);
  rpc GetDonor(GetDonorRequest) returns (Donor);
}
```

**Database Schema**:
```sql
CREATE TABLE Categories (
  Id INT PRIMARY KEY IDENTITY(1,1),
  Name NVARCHAR(50) NOT NULL UNIQUE,
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE Donors (
  Id INT PRIMARY KEY IDENTITY(1,1),
  Name NVARCHAR(50) NOT NULL,
  Email NVARCHAR(50) NOT NULL UNIQUE,
  Phone NVARCHAR(15) NOT NULL,
  Contribution DECIMAL(10,2),
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE Gifts (
  Id INT PRIMARY KEY IDENTITY(1,1),
  Name NVARCHAR(100) NOT NULL UNIQUE,
  Description NVARCHAR(200) NOT NULL,
  Price DECIMAL(10,2) NOT NULL,
  CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(Id),
  DonorId INT NOT NULL FOREIGN KEY REFERENCES Donors(Id),
  ImagePath NVARCHAR(200) NOT NULL,
  WinnerId INT NULL FOREIGN KEY REFERENCES Users(Id),  -- Reference to Auth Service
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  UpdatedAt DATETIME
);
```

**Dependencies**: 
- Auth Service (for [Authorize] claims)
- Sales Service (for WinnerId updates—via events or gRPC)

**Notes**:
- WinnerId is a foreign key reference to Auth Service's Users table—handled via gRPC or event propagation
- High read traffic (catalog browsing); implement caching layer (Redis) for categories and popular gifts
- Image assets require shared storage strategy (Azure Blob Storage, S3, or NFS with asset synchronization)

---

### 3. Sales Service

**Purpose**: Manage shopping basket and purchase transactions

**Responsibilities**:
- Basket item management (add, update, remove)
- Basket retrieval for current user
- Purchase processing and order history
- Order fulfillment coordination
- Transaction validation and business rules

**Owned Data**:
- Baskets table (Id, UserId, GiftId, Amount, CreatedAt, UpdatedAt)
- Purchases table (Id, CustomerId, GiftId, PurchDate, Status)

**Current Code Mapping**:
- BasketController.cs: Basket endpoints
- PurchasesController.cs: Purchase history endpoints
- BasketService.cs, PurchasesService.cs: Business logic
- BasketRepository.cs, PurchasesRepository.cs: Data access
- Basket.cs, Purchase.cs models

**API Contracts** (REST):
```
GET    /basket              → BasketDTO[] [Requires JWT]
POST   /basket              → CreateBasketDto → BasketDTO [Requires JWT]
PUT    /basket/{id}/amount  → { newAmount: int } → BasketDTO [Requires JWT]
DELETE /basket/{id}         → BasketDTO [Requires JWT]
POST   /basket/buy-all      → void → { success: bool } [Requires JWT]

GET    /purchases           → PurchaseDTO[] [Requires JWT]
GET    /purchases/history   → PagedResponse<PurchaseDTO> [Requires JWT]
```

**gRPC Contracts** (for internal calls):
```
service SalesService {
  rpc VerifyPurchase(VerifyPurchaseRequest) returns (VerifyPurchaseResponse);
  rpc GetPurchaseHistory(GetPurchaseHistoryRequest) returns (PurchaseHistoryResponse);
  rpc CompletePurchase(CompletePurchaseRequest) returns (CompletePurchaseResponse);
}
```

**Database Schema**:
```sql
CREATE TABLE Baskets (
  Id INT PRIMARY KEY IDENTITY(1,1),
  UserId INT NOT NULL,  -- Reference to Auth Service Users
  GiftId INT NOT NULL,  -- Reference to Gift Management Service Gifts
  Amount INT NOT NULL CHECK (Amount > 0),
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  UpdatedAt DATETIME
);

CREATE TABLE Purchases (
  Id INT PRIMARY KEY IDENTITY(1,1),
  CustomerId INT NOT NULL,  -- Reference to Auth Service Users
  GiftId INT NOT NULL,  -- Reference to Gift Management Service Gifts
  PurchDate DATETIME NOT NULL,
  Status NVARCHAR(20) NOT NULL,  -- 'Pending', 'Completed', 'Cancelled'
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);
```

**Dependencies**:
- Auth Service (for UserId/CustomerId validation and authorization)
- Gift Management Service (for GiftId validation and gift pricing)
- Lottery Service (for winner assignment after successful purchase)

**Notes**:
- Basket is user-scoped; enforce ownership via JWT claims
- Purchase completion triggers distributed transaction (Saga) coordinating with Gift and Lottery services
- High write volume during promotional periods; consider connection pooling optimization

---

### 4. Lottery Service

**Purpose**: Manage lottery draws, winner selection, and gift assignments

**Responsibilities**:
- Lottery draw execution (random winner selection from purchasers)
- Winner validation and eligibility checks
- Gift assignment to winners
- Draw history and audit trail
- Notification orchestration (publish events for email/SMS)

**Owned Data**:
- LotteryDraws table (Id, DrawDate, TotalWinners, ExecutedAt, Status)
- WinnerAssignments table (Id, DrawId, WinnerId, GiftId, AssignedAt)
- EligibilityRecords table (for tracking participant eligibility)

**Current Code Mapping**:
- LotteryController.cs: Lottery endpoints
- LotteryService.cs: Draw logic
- LotteryRepository.cs: Data access

**API Contracts** (REST):
```
POST   /lottery/draw        → { drawCount: int } → DrawResultDTO [Admin Only]
GET    /lottery/winners     → WinnerAssignmentDTO[] [Admin Only]
GET    /lottery/draw/{id}   → LotteryDrawDTO [Admin Only]
POST   /lottery/assign      → { giftId: int, winnerId: int } → AssignmentDTO [Admin Only]
```

**Event Contracts** (Published to RabbitMQ):
```
LotteryDrawExecuted {
  drawId: uuid
  executedAt: datetime
  totalWinners: int
  assignments: [
    { giftId: int, winnerId: int, userEmail: string }
  ]
}

GiftAssignedToWinner {
  giftId: int
  winnerId: int
  assignedAt: datetime
}
```

**Database Schema**:
```sql
CREATE TABLE LotteryDraws (
  Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  DrawDate DATETIME NOT NULL,
  TotalWinners INT NOT NULL,
  ExecutedAt DATETIME,
  Status NVARCHAR(20) NOT NULL,  -- 'Scheduled', 'Executing', 'Completed', 'Failed'
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE WinnerAssignments (
  Id INT PRIMARY KEY IDENTITY(1,1),
  DrawId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES LotteryDraws(Id),
  WinnerId INT NOT NULL,  -- Reference to Auth Service Users
  GiftId INT NOT NULL,    -- Reference to Gift Management Service Gifts
  AssignedAt DATETIME NOT NULL,
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE EligibilityRecords (
  Id INT PRIMARY KEY IDENTITY(1,1),
  UserId INT NOT NULL,  -- Reference to Auth Service Users
  PurchaseCount INT NOT NULL,
  EligibleFrom DATETIME NOT NULL,
  EligibleUntil DATETIME,
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);
```

**Event Subscriptions** (Consumes from RabbitMQ):
```
PurchaseCompleted → Update EligibilityRecords
UserRegistered → Initialize eligibility tracking
```

**Dependencies**:
- Auth Service (for WinnerId validation, email lookup)
- Gift Management Service (for GiftId validation, winner assignment)
- Sales Service (for purchase eligibility calculations)

**Notes**:
- Draw execution is a batch operation; should be idempotent and handle partial failures
- Winner assignment to gift requires coordination with Gift Management Service (update WinnerId)
- Asynchronous event publishing decouples Lottery from downstream notification systems

---

## Communication Strategy

### Overview

Services communicate via three primary channels:
1. **Synchronous REST** — For client-facing API and simple request-response patterns
2. **Synchronous gRPC** — For high-performance, internal service-to-service calls
3. **Asynchronous Events (Message Broker)** — For eventual consistency and non-blocking operations

---

### REST (Client ↔ API Gateway)

**When to Use**:
- Client requests through API Gateway
- Simple request-response with clear success/failure
- Stateless, idempotent operations

**Example**:
```
Client → API Gateway → Auth Service
POST /auth/login with credentials → { token: "jwt..." }
```

**Protocol**: HTTPS with Bearer JWT token in Authorization header

**Error Handling**: Standard HTTP status codes (200, 201, 400, 401, 403, 404, 500)

---

### gRPC (Service ↔ Service)

**When to Use**:
- Internal service-to-service communication
- High-frequency calls (e.g., token validation per request)
- Type-safe, strongly-typed contracts
- Performance-critical paths

**Technology Stack**:
- gRPC protocol (HTTP/2 multiplexing)
- Protocol Buffers (.proto files) for schema definition
- Shared language: C# with Grpc.AspNetCore NuGet

**Example: Token Validation**

Auth Service exposes gRPC endpoint:
```
API Gateway → Auth Service (gRPC)
ValidateTokenRequest { token: string } → ValidateTokenResponse { valid: bool, userId: int, role: string }
```

**Connection Management**:
- Long-lived gRPC channels per service
- Service discovery via DNS (service names in Docker Compose: `auth-service:50051`)
- Kubernetes service discovery in production (automatic DNS resolution)

**Security**: mTLS (mutual TLS) for encrypted, authenticated connections between services

---

### RabbitMQ (Asynchronous Events)

**When to Use**:
- Cross-service notifications (eventual consistency acceptable)
- Non-blocking, background operations
- Fan-out scenarios (one event → multiple subscribers)

**Technology Stack**:
- RabbitMQ message broker
- MassTransit or NServiceBus for .NET integration
- JSON-based event contracts

**Key Events**:

1. **UserRegistered**
   - Publisher: Auth Service
   - Subscribers: Lottery Service (initialize eligibility)
   - Payload: `{ userId: int, email: string, registeredAt: datetime }`

2. **PurchaseCompleted**
   - Publisher: Sales Service
   - Subscribers: Gift Management Service (update inventory), Lottery Service (update eligibility)
   - Payload: `{ purchaseId: int, userId: int, giftId: int, amount: int, completedAt: datetime }`

3. **LotteryDrawExecuted**
   - Publisher: Lottery Service
   - Subscribers: Gift Management Service (assign winner), Notification Service (send emails)
   - Payload: `{ drawId: uuid, assignments: [{ giftId, winnerId }], executedAt: datetime }`

4. **GiftWinnerAssigned**
   - Publisher: Gift Management Service
   - Subscribers: Notification Service (alert winner)
   - Payload: `{ giftId: int, winnerId: int, assignedAt: datetime }`

**Guarantees**:
- At-least-once delivery (with idempotency keys for duplicate handling)
- Dead-letter queue for failed message processing
- Message replay capability for audit trail

**Example Flow**:
```
Sales Service completes purchase
  ↓
Publishes "PurchaseCompleted" event to RabbitMQ
  ↓
Gift Management Service receives → decrements inventory
Lottery Service receives → updates eligibility count
Notification Service receives → queues confirmation email
```

---

### Communication Pattern Matrix

| Scenario | Protocol | Reason |
|----------|----------|--------|
| Client login | REST | Stateless, clear user interaction |
| API Gateway validates JWT | gRPC | High frequency, must be fast |
| Purchase workflow | Event + gRPC | Async coordination + atomic operations |
| List gifts by category | REST | Cacheable, client-driven |
| Assign winner to gift | gRPC | Requires immediate consistency |
| Notify user of lottery result | Event | Fire-and-forget, background job |

---

## Data Strategy & Consistency

### Database per Service

Each microservice owns an isolated SQL Server database. This approach:
- **Eliminates shared schema coupling** — Services evolve independently
- **Enables independent scaling** — Each DB sized for its workload
- **Supports polyglot persistence** — Future flexibility to use NoSQL where appropriate

**Database Allocation**:

| Service | Database | Purpose |
|---------|----------|---------|
| Auth Service | `Chinese_Auth_DB` | Users, roles, authentication state |
| Gift Management | `Chinese_GiftCatalog_DB` | Gifts, categories, donors, inventory |
| Sales Service | `Chinese_Sales_DB` | Baskets, purchases, order history |
| Lottery Service | `Chinese_Lottery_DB` | Draws, assignments, eligibility |

**Connection Strings**: Stored in Azure Key Vault or environment variables per deployment

---

### Data Consistency Strategies

**Challenge**: Distributed data across service boundaries requires careful orchestration. Unlike monolith with ACID transactions, microservices must handle eventual consistency.

#### Strategy A: Saga Pattern (Recommended Initially)

**Concept**: Multi-step transaction orchestrated across services with compensating transactions for rollback.

**Choreography vs. Orchestration**:
- **Choreography** (Event-driven): Services react to events; no central coordinator
- **Orchestration** (Controller-based): Central saga coordinator directs service calls

**Recommended**: Orchestration via Saga Coordinator service for visibility and easier debugging.

**Example: "Purchase & Assign Winner" Saga**

```
Step 1: Sales Service validates basket
  → If fails: Return error to client
  
Step 2: Saga Coordinator requests Purchase Completion
  → Sales Service marks purchase as "Pending"
  
Step 3: Saga Coordinator calls Gift Management Service
  → Update gift record with purchase reference
  → If fails: Compensate (revert purchase to draft)
  
Step 4: Saga Coordinator publishes "PurchaseCompleted" event
  → Lottery Service updates eligibility counts
  
Step 5: On success
  → Publish "PurchaseFinalized" event → Notification Service sends confirmation
  
Step 6: On failure at any step
  → Execute compensating transactions in reverse order
  → Publish "PurchaseCancelled" event
```

**Implementation**:
- Use MassTransit or NServiceBus for saga orchestration
- Define saga state machine in code
- Idempotent service operations (safe to retry)
- Timeout handling for stuck sagas

**Advantages**:
- Guarantees consistency across services
- Clear error handling and rollback semantics
- Visible transaction flow

**Disadvantages**:
- Added latency (multiple service calls sequentially)
- Complexity in saga coordination logic
- Requires idempotency in all service operations

---

#### Strategy B: Outbox Pattern (Alternative)

**Concept**: Service writes data + events to local database atomically, background worker reliably publishes events.

**Flow**:
1. Service performs business operation and inserts event into local Outbox table
2. All changes committed atomically (single DB transaction)
3. Background worker polls Outbox table, publishes events to RabbitMQ
4. Once published, event marked as processed and deleted

**Advantages**:
- No two-phase commit; guaranteed event publication
- Decoupled from RabbitMQ availability
- Simpler than saga orchestration

**Disadvantages**:
- Eventual consistency (slight delay before subscribers see events)
- Requires background worker per service
- More complex infrastructure

---

### Choosing Between Saga and Outbox

**Use Saga Pattern When**:
- Immediate consistency required (e.g., inventory must not oversell)
- Transaction spans 3+ services
- Compensating transactions are feasible

**Use Outbox Pattern When**:
- Eventual consistency acceptable (minutes to hours)
- Publishing reliability is critical
- Services handle duplicates gracefully

**For Chinese Sales System**:
- **Saga**: Purchase completion (must be atomic across Sales + Gift services)
- **Outbox**: Lottery notifications (eventual consistency acceptable)

---

### Handling Cross-Service References

**Problem**: Foreign key relationships now span databases.

**Solutions**:

1. **Denormalization via Events**
   - Lottery Service stores user name/email (cached copy of Auth Service data)
   - Updated via "UserProfileUpdated" events
   - Eventual consistency acceptable for display data

2. **Reference Integrity via gRPC**
   - Before inserting purchase referencing GiftId, verify gift exists (gRPC call to Gift Service)
   - Accept stale reads for better performance

3. **Unique Identifiers (GUIDs)**
   - All cross-service references use globally-unique IDs (GUID or UUID)
   - No reliance on sequential IDs from other services

---

## Shared Infrastructure

### API Gateway

**Purpose**: Single entry point for all client requests; handles routing, authentication, and cross-cutting concerns.

**Responsibilities**:
- JWT validation (calls Auth Service via gRPC)
- Route requests to appropriate microservice
- Rate limiting and throttling
- CORS policy enforcement
- Request logging with correlation IDs
- Response aggregation (if needed)

**Technology Options**:
1. **Ocelot** (ASP.NET Core library) — Lightweight, configuration-driven, good for greenfield
2. **Kong** (API Gateway appliance) — Full-featured, but operational overhead
3. **Custom ASP.NET Core Middleware** — Maximum control, existing team expertise

**Recommended**: Ocelot for simplicity + existing ASP.NET Core knowledge.

**Configuration Example** (ocelot.json):
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/auth/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [{ "Host": "auth-service", "Port": 443 }],
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "UpstreamHttpMethod": ["POST", "GET"],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" },
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1m",
        "PeriodTimespan": 60,
        "Limit": 100
      }
    },
    {
      "DownstreamPathTemplate": "/gifts/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [{ "Host": "gift-service", "Port": 443 }],
      "UpstreamPathTemplate": "/api/gifts/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    }
  ]
}
```

**JWT Validation at Gateway**:
```csharp
// Gateway fetches Auth Service's public key (cached)
var publicKey = await authServiceClient.GetPublicKeyAsync();

// Validate token signature
var principal = ValidateJwtToken(token, publicKey);

// Extract claims, add to HttpContext
HttpContext.Items["UserId"] = principal.FindFirst("id")?.Value;
HttpContext.Items["UserRole"] = principal.FindFirst("role")?.Value;

// Forward to downstream service (service trusts gateway's claims)
```

**Downstream Services**: Accept claims from gateway headers; no re-validation needed.

---

### Service Discovery

**In Development** (Docker Compose):
- Service names resolve via Docker's internal DNS
- Example: `http://auth-service:5000` within Docker network

**In Production** (Kubernetes):
- Kubernetes DNS automatically resolves service names
- Example: `http://auth-service.default.svc.cluster.local:5000`

**For gRPC Channels**:
```csharp
// Automatic retry and load balancing
var channel = GrpcChannel.ForAddress("https://auth-service:50051", 
  new GrpcChannelOptions { Credentials = ChannelCredentials.SecureSsl });
var client = new AuthService.AuthServiceClient(channel);
```

---

### Centralized Logging

**Architecture**:
- Each service: Serilog configured with structured logging
- Sink: Elasticsearch for centralized storage
- UI: Kibana for log visualization and analysis
- Correlation: Unique request ID propagated across all service calls

**Implementation**:

1. **Serilog Configuration** (consistent across all services):
```csharp
Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("https://elasticsearch:9200"))
  {
    AutoRegisterTemplate = true,
    IndexFormat = "logs-{0:yyyy.MM.dd}"
  })
  .Enrich.WithProperty("ServiceName", "AuthService")
  .Enrich.FromLogContext()
  .CreateLogger();
```

2. **Correlation ID Propagation**:
```csharp
// Middleware in API Gateway
app.Use(async (context, next) =>
{
  var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var value)
    ? value.ToString()
    : Guid.NewGuid().ToString();
  
  context.Items["CorrelationId"] = correlationId;
  using (LogContext.PushProperty("CorrelationId", correlationId))
  {
    await next();
  }
});

// gRPC metadata (service-to-service)
var metadata = new Metadata
{
  { "x-correlation-id", correlationId }
};
var response = await client.ValidateTokenAsync(request, metadata);
```

3. **Query Example** (Kibana):
```
ServiceName:AuthService AND CorrelationId:"12345-67890" 
→ See all logs for a single user transaction across services
```

**Advantages of Centralized Logging**:
- Traceability: Follow a single request through all services
- Troubleshooting: Correlate errors across service logs
- Compliance: Centralized audit trail for regulatory requirements
- Existing Pattern: Builds on current LoggingHelper utility

---

### Configuration Management

**Per-Service Configuration**:
```
appsettings.json          (default settings)
appsettings.Production.json (production overrides)
```

**Secrets Management**:
- **Dev**: Local user secrets or environment variables
- **Production**: Azure Key Vault or HashiCorp Vault

**Example**:
```csharp
builder.Configuration
  .AddJsonFile("appsettings.json")
  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
  .AddAzureKeyVault(
    vaultUri: new Uri(keyVaultUrl),
    credential: new DefaultAzureCredential())
  .AddEnvironmentVariables();
```

**No Hardcoded Secrets** in code, configuration files, or documentation.

---

## Security & Authentication

### JWT Token Flow

**Overview**: Tokens issued by Auth Service, validated at Gateway, claims propagated to downstream services.

```
┌─────────┐
│ Client  │
└────┬────┘
     │ 1. POST /auth/login (credentials)
     ↓
┌──────────────┐
│ API Gateway  │
└────┬─────────┘
     │ 2. Route to Auth Service
     ↓
┌───────────────────┐
│ Auth Service      │
│ - Validate creds  │
│ - Generate JWT    │
└────┬──────────────┘
     │ 3. Return JWT
     ↓
┌──────────────┐
│ API Gateway  │
│ - Cache JWT  │
│ - Extract    │
│   claims     │
└────┬─────────┘
     │ 4. Forward to service + claims headers
     ↓
┌──────────────────────┐
│ Downstream Service   │
│ - Trust claims       │
│ - Enforce authz      │
└──────────────────────┘
```

### Token Validation at Gateway

**Cached Public Key** (Auth Service signs with private key, Gateway validates with public key):

```csharp
// Auth Service exposes public key endpoint
GET /auth/.well-known/keys → { keys: [{ kid, kty, use, n, e }] }

// Gateway caches and validates JWT signature
var tokenHandler = new JwtSecurityTokenHandler();
var validationParameters = new TokenValidationParameters
{
  ValidateIssuerSigningKey = true,
  IssuerSigningKey = new JsonWebKeySet(...).Keys.First(), // from cache
  ValidateIssuer = true,
  ValidIssuer = "Chinese_Sales_API",
  ValidateAudience = true,
  ValidAudience = "chinese-sales-clients",
  ClockSkew = TimeSpan.Zero
};

var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
```

**Claims in JWT**:
```json
{
  "id": "42",
  "name": "john.doe",
  "email": "john@example.com",
  "role": "User",
  "phone": "+86-123-4567",
  "iat": 1620000000,
  "exp": 1620003600
}
```

### Authorization

**At Gateway Level**:
```csharp
// Extract role from JWT claims
var role = principal.FindFirst("role")?.Value;

// Check endpoint authorization
if (endpoint.RequiredRole == "Admin" && role != "Admin")
{
  return Unauthorized();
}
```

**At Service Level**:
Each service enforces domain-specific authorization:
```csharp
[HttpDelete("/gifts/{id}")]
[Authorize(Roles = "Admin")]  // Still useful for documentation
public async Task<IActionResult> DeleteGift(int id)
{
  // Verify gift exists, user is admin, then delete
  var gift = await service.DeleteGiftAsync(id);
  return Ok(gift);
}
```

### Service-to-Service Security (gRPC)

**Mutual TLS (mTLS)**:
```csharp
// Sales Service calling Gift Management Service
var httpClientHandler = new HttpClientHandler();
httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
httpClientHandler.ClientCertificates.Add(clientCertificate);

var handler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, httpClientHandler);
var channel = GrpcChannel.ForAddress("https://gift-service:50051", 
  new GrpcChannelOptions { HttpHandler = handler });
var client = new GiftService.GiftServiceClient(channel);
```

### Secrets & Sensitive Data

**Strictly Forbidden** in this document:
- ❌ Connection strings
- ❌ JWT secret keys
- ❌ API keys
- ❌ Database passwords
- ❌ Redis credentials

**Handling**:
- Stored in secure vaults (Azure Key Vault, Vault, AWS Secrets Manager)
- Accessed at runtime via secure configuration providers
- Rotated regularly (no manual rotation)
- Audited for access

---

## Data Aggregation & Reporting Strategies

### The Challenge: Cross-Database Queries

**Problem**: In the monolithic system, a single SQL query could JOIN users, gifts, purchases, and lottery records. With microservices, this data is scattered across four databases.

**Example Queries That Become Hard**:
1. "List all gifts with their donor names and current bid count"
2. "Show winner leaderboard with gift details"
3. "Generate admin dashboard with total revenue by category"

**Why This Matters**:
- Reporting/analytics often require data from multiple domains
- Denormalizing data into each service wastes storage and creates consistency issues
- Querying one service at a time and merging in-memory is slow and error-prone

---

### Solution A: API Composition (Simple, Synchronous)

**Concept**: A dedicated "Reporting Service" or the API Gateway calls multiple services and merges responses in memory.

**When to Use**:
- Small result sets (< 10,000 records)
- Low-frequency queries (admin dashboards, not per-request)
- Real-time freshness required
- Simple aggregations (joins, filters, sorting)

**Architecture**:
```
Reporting Service (or API Gateway)
  ↓
  Calls Auth Service for user profiles
  Calls Gift Service for gift catalogs
  Calls Sales Service for purchase history
  ↓
  Merges results in memory
  ↓
  Returns aggregated response to client
```

**Example Implementation** (C#):
```csharp
[HttpGet("/reports/winners")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetWinnersReport()
{
  // Call Lottery Service for winners
  var winners = await lotteryClient.GetWinnersAsync();
  
  // For each winner, fetch user details from Auth Service
  var userIds = winners.Select(w => w.WinnerId).Distinct();
  var users = await authClient.GetUsersByIdsAsync(userIds);
  
  // For each winner, fetch gift details from Gift Service
  var giftIds = winners.Select(w => w.GiftId).Distinct();
  var gifts = await giftClient.GetGiftsByIdsAsync(giftIds);
  
  // Merge in memory
  var report = winners.Select(w => new WinnerReportDto
  {
    WinnerId = w.WinnerId,
    WinnerName = users.First(u => u.Id == w.WinnerId).Name,
    GiftName = gifts.First(g => g.Id == w.GiftId).Name,
    GiftPrice = gifts.First(g => g.Id == w.GiftId).Price,
    AssignedAt = w.AssignedAt
  }).ToList();
  
  return Ok(report);
}
```

**Advantages**:
- ✅ Real-time data freshness (always current)
- ✅ Simple implementation (no new infrastructure)
- ✅ Works with existing services without modification
- ✅ Can implement caching for slow endpoints

**Disadvantages**:
- ❌ Multiple network round-trips (latency adds up)
- ❌ Doesn't scale for large datasets (in-memory merge)
- ❌ Cascading failures (if one service slow, entire query times out)
- ❌ N+1 query problem (loop to fetch related data)

**Recommended For**:
- Admin reports run on demand (not frequent)
- Small result sets
- Real-time accuracy critical

---

### Solution B: CQRS & Read Models (Complex, Asynchronous)

**Concept**: Maintain a denormalized "Read Database" (Elasticsearch, dedicated SQL Server) that listens to domain events and stays in sync.

**When to Use**:
- Large result sets (100,000+ records)
- High-frequency queries (customer-facing dashboards)
- Complex aggregations and full-text search
- Performance is critical
- Eventual consistency acceptable (seconds to minutes lag)

**Architecture**:
```
Event Stream (RabbitMQ)
  ↓
  PurchaseCompleted event
  LotteryDrawExecuted event
  UserRegistered event
  ↓
Read Model Projector (background worker)
  ↓
Updates Read Database (Elasticsearch or SQL)
  ↓
Reporting Service queries Read DB
  ↓
Client gets instant results
```

**Data Flow Example**:

1. **Lottery Service executes draw**, publishes:
```json
{
  "event_type": "LotteryDrawExecuted",
  "event_id": "draw-2026-05-13-001",
  "timestamp": "2026-05-13T10:00:00Z",
  "data": {
    "draw_id": "uuid-xxx",
    "assignments": [
      { "user_id": 42, "gift_id": 15, "user_name": "Alice", "gift_name": "Laptop", "gift_price": 1200 }
    ]
  }
}
```

2. **Read Model Projector** consumes event:
```csharp
public async Task HandleLotteryDrawExecutedAsync(LotteryDrawExecutedEvent evt)
{
  foreach (var assignment in evt.Data.Assignments)
  {
    var readModel = new WinnerReadModel
    {
      DrawId = evt.Data.DrawId,
      UserId = assignment.UserId,
      UserName = assignment.UserName,  // Denormalized
      GiftId = assignment.GiftId,
      GiftName = assignment.GiftName,   // Denormalized
      GiftPrice = assignment.GiftPrice, // Denormalized
      AssignedAt = evt.Timestamp
    };
    
    await elasticsearchRepository.IndexAsync(readModel);
  }
}
```

3. **Reporting Service queries Read DB**:
```csharp
[HttpGet("/reports/winners")]
public async Task<IActionResult> GetWinnersReport([FromQuery] int page = 1)
{
  // Instant query on denormalized data
  var winners = await elasticsearchRepository.SearchWinnersAsync(
    filters: null,
    page: page,
    pageSize: 50
  );
  
  return Ok(winners);
}
```

**Denormalized Read Model Schema** (Elasticsearch):
```json
{
  "mappings": {
    "properties": {
      "draw_id": { "type": "keyword" },
      "user_id": { "type": "integer" },
      "user_name": { "type": "text" },
      "user_email": { "type": "keyword" },
      "gift_id": { "type": "integer" },
      "gift_name": { "type": "text" },
      "gift_price": { "type": "float" },
      "category_name": { "type": "keyword" },
      "donor_name": { "type": "keyword" },
      "assigned_at": { "type": "date" },
      "timestamp": { "type": "date" }
    }
  }
}
```

**Handling Projection Failures** (Resilience):

If projector crashes mid-update:
1. Idempotency: Replay same event → same result (use event_id as document ID)
2. Dead-letter queue: Failed events go to DLQ for manual replay
3. Reconciliation job: Periodic background job verifies read model matches source of truth

**Advantages**:
- ✅ Lightning-fast queries (indexed, denormalized data)
- ✅ Scales to large datasets (database-level optimization)
- ✅ Complex aggregations and filters (search engine capabilities)
- ✅ No cascading failures (read DB independent)
- ✅ Full-text search capability (Elasticsearch)
- ✅ Real-time updates via event stream

**Disadvantages**:
- ❌ Eventual consistency (seconds lag acceptable?)
- ❌ Projection failures cause stale data (needs monitoring)
- ❌ More infrastructure (Elasticsearch, projector service)
- ❌ Denormalization maintenance (keeping read model in sync)
- ❌ Event replay complexity (if business logic changes)

**Recommended For**:
- Customer-facing dashboards
- Analytics and reporting features
- High-frequency queries
- Large datasets

---

### Implementation Recommendation for Chinese Sales System

**Phase 1 (Initial)**:
- **Simple admin reports**: API Composition
  - "Donors" admin dashboard: Call Donor Service + Gift Service
  - "Recent Purchases": Call Sales Service directly
  - "Draw History": Call Lottery Service directly

**Phase 2 (Growth)**:
- **High-traffic dashboards**: CQRS + Read Models
  - Winner leaderboard (public): `SELECT TOP 100 FROM WinnerReadModel ORDER BY assigned_at DESC`
  - Category sales analytics: `SELECT category, SUM(price) FROM GiftReadModel GROUP BY category`
  - User purchase history: `SELECT * FROM PurchaseReadModel WHERE user_id = ? ORDER BY purchase_date DESC`

**Hybrid Approach**:
```
API Gateway receives GET /reports/winners
  ├─ Check cache (Elasticsearch read model)
  │  └─ If hit (< 1 min old): Return immediately
  │
  ├─ If miss or stale
  │  ├─ Call Lottery Service (gRPC) for latest assignments
  │  ├─ Enrich with user/gift details from Auth + Gift services
  │  ├─ Update cache
  │  └─ Return to client
```

**Progressive Adoption**:
1. Start with API Composition (simple, low overhead)
2. Monitor query latency (identify slow operations)
3. For slow queries, migrate to CQRS + Read Models
4. Monitor read model staleness (alert if > 5 min lag)

---

## Migration Roadmap

### Phase 1: Foundation (Months 1-2)

**Deliverables**:
1. API Gateway (Ocelot) setup
2. Auth Service extraction (isolated database)
3. JWT validation at gateway level
4. Service discovery configuration (Docker Compose, Kubernetes templates)

**Testing**: End-to-end tests through gateway

**Deployment**: Dev/Staging environments

---

### Phase 2: Core Services (Months 3-4)

**Deliverables**:
1. Gift Management Service extraction
2. Sales Service extraction
3. Inter-service gRPC communication setup
4. Saga orchestrator for purchase workflow

**Milestones**:
- Gift CRUD endpoints functional
- Basket operations working
- Purchase saga coordinating services

**Testing**: Integration tests for saga workflows

---

### Phase 3: Lottery & Events (Month 5)

**Deliverables**:
1. Lottery Service extraction
2. RabbitMQ event infrastructure
3. Event projections (Outbox Pattern)
4. Asynchronous notifications

**Milestones**:
- Draw execution coordinated via saga
- Winner assignment publishing events
- Eligible users updated via events

---

### Phase 4: Observability & Scale (Month 6+)

**Deliverables**:
1. Centralized logging (ELK Stack)
2. Metrics and monitoring (Prometheus + Grafana)
3. Distributed tracing (Jaeger)
4. Performance testing and optimization

**Load Testing**: Simulated production traffic

**Production Deployment**: Gradual rollout with feature flags

---

### Data Migration Strategy

**Approach: Parallel Running (Strangler Pattern)**

1. **Dual-write phase**: Monolith writes to both old and new databases
2. **Validation phase**: Compare queries from old vs. new, ensure consistency
3. **Read-through phase**: Routes read requests to new services, fall back to monolith on errors
4. **Cutover phase**: All traffic → microservices, decommission monolith
5. **Cleanup**: Archive old data, shutdown old infrastructure

**Risk Mitigation**:
- Automated rollback if error rate exceeds threshold
- Data validation queries run continuously
- Keep monolith operational for 2+ weeks post-migration
- Automated alerts on schema drift

---

## Further Considerations

### 1. Donor Service Extraction (Future)

As noted in decomposition, **Donor management could be extracted into a standalone service** if:

- **Indicators for extraction**:
  - Donor operations become high-traffic (registrations, profile updates)
  - Separate team owns donor relations
  - Donor domain logic expands significantly (loyalty programs, payment reconciliation)
  - Gift Management team is release-blocked by donor features

- **Migration path**:
  - Extract Donor Service while Gift Service retains reference to DonorId
  - Gift Service caches donor names (updated via events)
  - Eventually, Gift Service queries donor details via gRPC if needed

- **No action needed now**: Include Donor in Gift Management Service for Phase 1-2.

### 2. Deployment & Orchestration

**Development**: Docker Compose for local setup

```yaml
version: '3.8'
services:
  api-gateway:
    build: ./APIGateway
    ports:
      - "5000:5000"
  
  auth-service:
    build: ./Services/AuthService
    environment:
      - ConnectionStrings__DefaultConnection=Server=auth-db;Database=Chinese_Auth_DB;...
    depends_on:
      - auth-db
  
  auth-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=YourSecurePassword123
  
  # ... other services ...
  
  rabbitmq:
    image: rabbitmq:3.12-management
    ports:
      - "5672:5672"
      - "15672:15672"
  
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.0.0
    environment:
      - discovery.type=single-node
```

**Production**: Kubernetes with Helm charts

```yaml
apiVersion: v1
kind: Service
metadata:
  name: auth-service
spec:
  selector:
    app: auth-service
  ports:
    - port: 443
      targetPort: 443
```

### 3. Monitoring & Observability

**Key Metrics to Track**:
- Service latency (p50, p95, p99)
- Error rates (4xx, 5xx per service)
- Event lag (time from publication to processing)
- Database connection pool utilization
- Cache hit ratios (Redis, read models)

**Alerting**:
- Alert if any service latency p99 > 1 second
- Alert if error rate > 1%
- Alert if event lag > 5 minutes
- Alert on service unavailability (circuit breaker open)

### 4. Testing Strategy

**Unit Tests**: Per service (unchanged from monolith)

**Integration Tests**: Service + database + external dependencies (mocked)

**Contract Tests**: Verify gRPC / REST contracts between services

**End-to-End Tests**: Full saga workflows through API Gateway

**Chaos Engineering**: Inject failures (service crash, network latency) to validate resilience

### 5. Team Structure & Communication

**Recommended Organization**:
- Auth Team: Owns Auth Service
- Commerce Team: Owns Gift Management + Sales Services
- Lottery Team: Owns Lottery Service
- Platform Team: Owns API Gateway, shared infrastructure, logging

**Communication**:
- Async: Confluence pages for service docs, event contracts documented in wiki
- Sync: Weekly architecture sync, service-to-service API change reviews
- Contracts: Published in shared gRPC / REST documentation (Swagger, ProtoRegistry)

### 6. Security Scanning & Compliance

**Continuous Security**:
- SAST (Static Application Security Testing): Analyze code for vulnerabilities (SonarQube)
- Dependency scanning: Monitor NuGet packages for CVEs (Dependabot)
- Container scanning: Scan Docker images before deployment (Trivy)
- Secrets scanning: Ensure no credentials committed (TruffleHog)

**Compliance**:
- GDPR: Data residency, right to deletion implemented across services
- Audit logging: All user actions logged with timestamps and user IDs
- Encryption: TLS for all network traffic, encryption at rest for databases

---

## Conclusion

This microservices architecture provides a scalable, maintainable foundation for the Chinese Sales API. By decomposing the monolith into domain-driven services with clear boundaries, the system gains:

1. **Scalability**: Services scale independently
2. **Autonomy**: Teams own services end-to-end
3. **Resilience**: Failure isolation and graceful degradation
4. **Flexibility**: Technology choices per service
5. **Operational Visibility**: Centralized logging and monitoring

The recommended phased approach (6 months) balances speed with risk, allowing parallel work and early validation before full production deployment.

**Next Steps**:
1. Establish architecture review board (ARB)
2. Socialize design with stakeholders
3. Begin Phase 1 (API Gateway + Auth Service)
4. Establish team structure and communication protocols
5. Set up observability infrastructure (logging, metrics, tracing)

---

## Appendix: Reference Materials

### A. Technology Stack Summary

| Component | Technology | Version |
|-----------|-----------|---------|
| Services | ASP.NET Core | 8.0+ |
| Service-to-Service | gRPC + Protocol Buffers | v3 |
| API Gateway | Ocelot | 20.0+ |
| Databases | SQL Server | 2022+ |
| Cache | Redis | 7.0+ |
| Message Broker | RabbitMQ | 3.12+ |
| Logging | Serilog | 3.0+ |
| Log Sink | Elasticsearch | 8.0+ |
| Log Visualization | Kibana | 8.0+ |
| Service Discovery | Docker DNS / Kubernetes | - |
| Metrics | Prometheus | 2.40+ |
| Visualization | Grafana | 9.0+ |
| Distributed Tracing | Jaeger | 1.40+ |

### B. gRPC Service Definition Template

```protobuf
syntax = "proto3";

package chineseSalesAPI.auth;

service AuthService {
  rpc ValidateToken(ValidateTokenRequest) returns (ValidateTokenResponse);
  rpc GetUserClaims(GetUserClaimsRequest) returns (UserClaimsResponse);
  rpc GetPublicKey(Empty) returns (PublicKeyResponse);
}

message ValidateTokenRequest {
  string token = 1;
}

message ValidateTokenResponse {
  bool valid = 1;
  string error_message = 2;
  UserClaims claims = 3;
}

message UserClaims {
  int32 id = 1;
  string name = 2;
  string email = 3;
  string role = 4;
}

message GetUserClaimsRequest {
  int32 user_id = 1;
}

message UserClaimsResponse {
  int32 id = 1;
  string name = 2;
  string email = 3;
  string phone = 4;
  string role = 5;
}

message Empty {}

message PublicKeyResponse {
  string key = 1;
  string kid = 2;
}
```

### C. Event Schema Template (JSON)

```json
{
  "event_metadata": {
    "event_id": "uuid-v4",
    "event_type": "PurchaseCompleted",
    "event_version": "1.0",
    "published_at": "2026-05-13T10:00:00Z",
    "source_service": "SalesService",
    "correlation_id": "corr-12345-67890"
  },
  "event_data": {
    "purchase_id": 42,
    "customer_id": 15,
    "gift_id": 8,
    "amount": 3,
    "total_price": 3600.00,
    "currency": "CNY",
    "purchase_date": "2026-05-13T10:00:00Z",
    "status": "completed"
  }
}
```

### D. Service Health Check Endpoints

All services should expose a health check endpoint:

```csharp
app.MapGet("/health", async (ChineseSaleDbContext db) =>
{
  try
  {
    await db.Database.ExecuteSqlAsync($"SELECT 1");
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
  }
  catch
  {
    return Results.StatusCode(503);
  }
});
```

---

**Document Version**: 1.0  
**Last Updated**: May 13, 2026  
**Review Cycle**: Quarterly
