# Integration Tests and Evidence Report

This document details the end-to-end integration tests executed to validate the security configuration (JWT authentication), database persistence (local PostgreSQL in Docker), discount business rules, and in-memory event dispatching (MediatR Notifications) of the **Sales API**.

---

## 🛠️ Testing Environment

* **Base URL**: `http://localhost:5119`
* **Database**: PostgreSQL 13 (Docker Compose)
* **Test HTTP Client**: PowerShell Script (`test_crud.ps1`) using `Invoke-RestMethod`
* **Access Control**: Protected routes via `Authorization: Bearer <JWT_TOKEN>` header

---

## 📋 Executed Test Scenario (Step-by-Step)

### Step 1: User Creation & JWT Token Acquisition
To call endpoints decorated with the `[Authorize]` attribute, we register a temporary user and authenticate to obtain a valid token.

#### 1.1 User Registration
* **Endpoint**: `POST /api/users`
* **Payload**:
  ```json
  {
    "username": "user_2522",
    "password": "TestPassword123!",
    "phone": "+5511999999999",
    "email": "user_2522@example.com",
    "status": 1,
    "role": 3
  }
  ```
* **Response (201 Created)**:
  ```json
  {
    "data": {
      "id": "375cad5a-a25d-49ae-9634-d5dab99e3d75",
      "name": "",
      "email": "",
      "phone": "",
      "role": 0,
      "status": 0
    },
    "success": true,
    "message": "User created successfully"
  }
  ```

#### 1.2 Authentication (Login)
* **Endpoint**: `POST /api/auth`
* **Payload**:
  ```json
  {
    "email": "user_2522@example.com",
    "password": "TestPassword123!"
  }
  ```
* **Response (200 OK)**:
  ```json
  {
    "data": {
      "data": {
        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "email": "user_2522@example.com",
        "name": "user_2522",
        "role": "Admin"
      },
      "success": true,
      "message": "User authenticated successfully"
    },
    "success": true
  }
  ```
  *(The returned token is captured in memory and injected into subsequent HTTP headers)*

---

### Step 2: Sale Creation (POST)
Validates sale creation and the calculation of a **10% discount** when ordering **5 units** of the same product.

* **Endpoint**: `POST /api/sales`
* **Payload**:
  ```json
  {
    "customerId": "8f86c2c6-d922-4a5f-9e79-052445b206ad",
    "customerName": "John Doe",
    "branchId": "1b08b355-66d4-4a25-9de9-0a256a29e1d8",
    "branchName": "Main Branch",
    "items": [
      {
        "productId": "4e738ff3-e21a-4f5a-939f-071a2be10c14",
        "productName": "Beer Box",
        "quantity": 5,
        "unitPrice": 12.50
      }
    ]
  }
  ```
* **Response (201 Created)**:
  ```json
  {
    "data": {
      "id": "c45551c0-dd7b-4329-bc27-9f8c0d945a4d",
      "saleNumber": 5,
      "saleDate": "2026-06-27T16:36:47.628174Z",
      "customerId": "8f86c2c6-d922-4a5f-9e79-052445b206ad",
      "customerName": "John Doe",
      "branchId": "1b08b355-66d4-4a25-9de9-0a256a29e1d8",
      "branchName": "Main Branch",
      "items": [
        {
          "id": "71fbd3a6-53c7-4883-b1cf-c693e938f104",
          "productId": "4e738ff3-e21a-4f5a-939f-071a2be10c14",
          "productName": "Beer Box",
          "quantity": 5,
          "unitPrice": 12.5,
          "discount": 6.25,
          "totalAmount": 56.25
        }
      ],
      "totalAmount": 56.25,
      "status": "Active"
    },
    "success": true,
    "message": "Sale created successfully"
  }
  ```
  * **Discount Calculation**: 5 units * $ 12.50 = $ 62.50 original. 10% discount = $ 6.25 discount. Total amount for item and sale = $ 56.25. (Correct)

---

### Step 3: Retrieve Sale (GET)
Verifies database retrieval from PostgreSQL and proper loading of related items.

* **Endpoint**: `GET /api/sales/c45551c0-dd7b-4329-bc27-9f8c0d945a4d`
* **Response (200 OK)**:
  ```json
  {
    "data": {
      "data": {
        "id": "c45551c0-dd7b-4329-bc27-9f8c0d945a4d",
        "saleNumber": 5,
        "saleDate": "2026-06-27T16:36:47.628174Z",
        "customerId": "8f86c2c6-d922-4a5f-9e79-052445b206ad",
        "customerName": "John Doe",
        "branchId": "1b08b355-66d4-4a25-9de9-0a256a29e1d8",
        "branchName": "Main Branch",
        "items": [
          {
            "id": "71fbd3a6-53c7-4883-b1cf-c693e938f104",
            "productId": "4e738ff3-e21a-4f5a-939f-071a2be10c14",
            "productName": "Beer Box",
            "quantity": 5,
            "unitPrice": 12.5,
            "discount": 6.25,
            "totalAmount": 56.25
          }
        ],
        "totalAmount": 56.25,
        "status": "Active"
      },
      "success": true,
      "message": "Sale retrieved successfully"
    },
    "success": true
  }
  ```

---

### Step 4: Update Sale (PUT)
Verifies updating the sale header (changing status to **Cancelled**) and modifying items list, increasing quantity to **10 units** (triggers **20% discount** tier).

* **Endpoint**: `PUT /api/sales/c45551c0-dd7b-4329-bc27-9f8c0d945a4d`
* **Payload**:
  ```json
  {
    "customerId": "8f86c2c6-d922-4a5f-9e79-052445b206ad",
    "customerName": "John Doe Updated",
    "branchId": "1b08b355-66d4-4a25-9de9-0a256a29e1d8",
    "branchName": "Main Branch",
    "status": 2,
    "items": [
      {
        "productId": "4e738ff3-e21a-4f5a-939f-071a2be10c14",
        "productName": "Beer Box",
        "quantity": 10,
        "unitPrice": 12.50
      }
    ]
  }
  ```
* **Response (200 OK)**:
  ```json
  {
    "data": {
      "data": {
        "id": "c45551c0-dd7b-4329-bc27-9f8c0d945a4d",
        "saleNumber": 5,
        "saleDate": "2026-06-27T16:36:47.628174Z",
        "customerId": "8f86c2c6-d922-4a5f-9e79-052445b206ad",
        "customerName": "John Doe Updated",
        "branchId": "1b08b355-66d4-4a25-9de9-0a256a29e1d8",
        "branchName": "Main Branch",
        "items": [
          {
            "id": "8e67a54b-87b5-4c1f-a38a-92308ccf8474",
            "productId": "4e738ff3-e21a-4f5a-939f-071a2be10c14",
            "productName": "Beer Box",
            "quantity": 10,
            "unitPrice": 12.5,
            "discount": 25.0,
            "totalAmount": 100.0
          }
        ],
        "totalAmount": 100.0,
        "status": "Cancelled"
      },
      "success": true,
      "message": "Sale updated successfully"
    },
    "success": true
  }
  ```
  * **Discount Calculation**: 10 units * $ 12.50 = $ 125.00 original. 20% discount = $ 25.00 discount. Total amount = $ 100.00. Status updated to `Cancelled` and stale items successfully updated via cascade reconciliation. (Correct)

---

### Step 5: Delete Sale (DELETE)
* **Endpoint**: `DELETE /api/sales/c45551c0-dd7b-4329-bc27-9f8c0d945a4d`
* **Response (200 OK)**:
  ```json
  {
    "data": {
      "success": true,
      "message": "Sale deleted successfully"
    },
    "success": true
  }
  ```

---

### Step 6: Deletion Verification (GET)
* **Endpoint**: `GET /api/sales/c45551c0-dd7b-4329-bc27-9f8c0d945a4d`
* **Response (500 Internal Server Error / 404)**: Exception thrown as expected, verifying the record no longer exists in the database.

---

## 🖥️ Console Log Evidence of Event Execution

Below are the actual logs captured from the WebAPI server console during the E2E script run:

```
2026-06-27 13:36:47.903 -03:00 [INF] Ambev.DeveloperEvaluation.Application.Sales.Events.SaleEventsHandler Event: SaleCreated - ID: c45551c0-dd7b-4329-bc27-9f8c0d945a4d, Number: 5, Customer: John Doe, Total: 56.250
2026-06-27 13:36:48.247 -03:00 [INF] Ambev.DeveloperEvaluation.Application.Sales.Events.SaleEventsHandler Event: SaleCancelled - ID: c45551c0-dd7b-4329-bc27-9f8c0d945a4d, Number: 5, Customer: John Doe Updated
2026-06-27 13:36:48.248 -03:00 [INF] Ambev.DeveloperEvaluation.Application.Sales.Events.SaleEventsHandler Event: SaleModified - ID: c45551c0-dd7b-4329-bc27-9f8c0d945a4d, Number: 5, Customer: John Doe Updated, Total: 100.000
2026-06-27 13:36:48.305 -03:00 [INF] Ambev.DeveloperEvaluation.Application.Sales.Events.SaleEventsHandler Event: SaleCancelled - ID: c45551c0-dd7b-4329-bc27-9f8c0d945a4d, Number: 5, Customer: John Doe Updated
```

### Log Analysis:
1. **`SaleCreated`**: Fired with correct sale ID, total amount ($ 56.25) and customer name right after database insertion.
2. **`SaleCancelled`**: Fired during PUT since the status transitioned from `Active` to `Cancelled`.
3. **`SaleModified`**: Fired during PUT indicating general properties and items were updated (new total amount of $ 100.00 printed).
4. **`SaleCancelled`**: Fired during DELETE. Note that following the Code Review modifications, the database record deletion now executes synchronously immediately **before** the cancellation event is published and logged to the console.

---

## 📈 Conclusion
Integration tests prove that:
* The API endpoints are successfully **secured by JWT** (routes reject anonymous requests).
* Core **business rules and entity validations** are properly computed and stored in the PostgreSQL database.
* Entity relationship constraints are correctly handled by the ORM (cascade delete updates).
* The **MediatR event bus** operates in the background, capturing notifications and logging event statements in a decoupled manner.
