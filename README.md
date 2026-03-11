# OpenIddict + Unified Blazor Web App POC

This repository contains a production-grade Proof of Concept (POC) for a centralized identity management system. It leverages **OpenIddict** to provide a secure, self-hosted OAuth2/OpenID Connect (OIDC) environment, serving as the single source of truth for both web and future mobile clients.

[Demo video](https://files.catbox.moe/m9brxc.webm)

## Architecture Overview

The solution is built on a decoupled architecture to ensure that the user's identity is managed independently of the application logic.

* **OSC.OpenIddict.AuthorizationServer:** The heart of the system. It handles user registration, local logins, and acts as an intermediary for Social SSO (Google, Facebook, Amazon).
* **OSC.OpenIddict.WebApi:** A secured resource server. It validates incoming Bearer tokens and enforces Role-Based Access Control (RBAC).
* **OSC.OpenIddict.Web (Blazor):** A unified Blazor Web App that uses the **Authorization Code Flow + PKCE** to authenticate users. It supports both Server and WASM rendering modes.
* **.NET MAUI Client:** *(Planned / In Development)*

---

## Tech Stack

* **Identity Framework:** [OpenIddict 5.7.0](https://documentation.openiddict.com/)
* **Frontend:** Blazor Unified (Interactive Auto)
* **ORM:** Entity Framework Core (SQL Server)
* **Auth Protocol:** OAuth 2.0 & OpenID Connect
* **Security:** Proof Key for Code Exchange (PKCE), Refresh Tokens, RSA-based Signing.

---

## Getting Started

### 1. Database Setup

The project uses **EF Core Migrations** to handle schema creation and seeding. This ensures that OpenIddict applications, scopes, and default roles (Admin/Member) are created automatically.

1. Ensure your connection string in `OSC.OpenIddict.AuthorizationServer/appsettings.json` points to a valid SQL Server instance.
2. Open the **Package Manager Console** in Visual Studio.
3. Set the `OSC.OpenIddict.AuthorizationServer` as the Default Project.
4. Run the following:

```powershell
Add-Migration InitialIdentityAndOpenIddict
Update-Database
```



### 2. External Provider Secrets

The POC supports Google, Facebook, and Amazon SSO. You must configure your Client IDs and Secrets in your environment variables or a `.env` file (if using a loader) or `appsettings.json`:

```json
"Authentication": {
  "Google": { "ClientId": "...", "ClientSecret": "..." },
  "Facebook": { "AppId": "...", "AppSecret": "..." },
  "Amazon": { "ClientId": "...", "ClientSecret": "..." }
}

```

---

## Implemented Features

### Membership & Identity

* **Local Accounts:** Full ASP.NET Core Identity integration (Hashed passwords, Email uniqueness).
* **Hybrid SSO:** Users can log in via external providers; the system automatically maps them to a local internal user record.
* **Identity UI:** Scaffolded and customized pages for Forgot Password, Reset Password, and Profile Management, optimized for the Blazor redirect lifecycle.

### Authorization & Roles

* **Role-Based Access Control (RBAC):** Roles (Admin/Member) are persisted in the database and issued as claims within the Access Token.
* **Scope Management:** Support for `openid`, `profile`, and custom API-specific scopes.
* **Token Lifecycle:** Full implementation of Refresh Tokens to allow for persistent login states without frequent re-authentication.

---

## Project Structure

```
.
├── OSC.OpenIddict.AuthorizationServer # OIDC Provider & Identity UI
├── OSC.OpenIddict.Web                 # Blazor Server Project
├── OSC.OpenIddict.Web.Client          # Blazor WASM components
├── OSC.OpenIddict.WebApi              # Secured Resource API
└── OSC.OpenIddict.slnx                # Visual Studio Solution
```

---

**Note:** This POC is intended for demonstration purposes. Ensure all RSA certificates and client secrets are rotated before moving toward a production environment.
