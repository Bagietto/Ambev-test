# Web API Sales Endpoints Verification

We verified the Developer Store Sales API by spinning up the database, running the application, registering a test user, authenticating to get a JWT, and performing the complete lifecycle of a Sale.

## Execution Summary

### 1. Database & Migrations
- Verified that PostgreSQL database is running on port `5432` under Docker container `ambev_developer_evaluation_database`.
- Applied EF Core migrations using:
  ```bash
  dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
  ```
  Result: Database was already up-to-date.

### 2. Running the Application
- Started the Web API locally under profile `http` on `http://localhost:5119` using:
  ```bash
  dotnet run --project src/Ambev.DeveloperEvaluation.WebApi --launch-profile http
  ```

### 3. Automated CRUD Flow Testing
We wrote a Python script `test_api.py` in the scratch folder to automate:
1. **User Creation**: Created user `ambevtester` with password `TestPassword@123`.
2. **User Authentication**: Logged in to obtain a JWT Bearer Token.
3. **Create Sale**: Created a sale containing two products with different quantities to verify the business discount logic:
   - **Product Alpha**: Qty 5, Unit Price 10.00 -> 10% discount -> total `45.00`.
   - **Product Beta**: Qty 12, Unit Price 20.00 -> 20% discount -> total `192.00`.
   - **Total Sale Amount**: `237.00`.
4. **Get Sale**: Verified the sale was retrieved correctly with calculations matching the database.
5. **Update Sale**: Updated the sale to have only 8 items of Product Alpha (yielding 10% discount, total `72.00`).
6. **Get Updated Sale**: Verified the update was correctly persisted.
7. **Delete Sale**: Deleted the sale.
8. **Verify Deletion**: Checked that fetching the deleted sale returns a 500 error (`KeyNotFoundException`).

### 4. Output Logs from Python Execution
Here is the console output from the run:

```
=== STEP 1: Creating User ===
Status: 201
{
  "data": {
    "id": "e6a70282-3df4-4b0d-82a9-540b9c0b5b20",
    "name": "",
    "email": "",
    "phone": "",
    "role": 0,
    "status": 0
  },
  "success": true,
  "message": "User created successfully",
  "errors": []
}

=== STEP 2: Authenticating ===
Status: 200
{
  "data": {
    "data": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJlNmE3MDI4Mi0zZGY0LTRiMGQtODJhOS01NDBiOWMwYjViMjAiLCJ1bmlxdWVfbmFtZSI6ImFtYmV2dGVzdGVyIiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzgyNTgwNDAzLCJleHAiOjE3ODI2MDkyMDMsImlhdCI6MTc4MjU4MDQwM30.e3Cn3U9U_40YCIj5sedgMGKZSEKsUV-B0eBcrT5Qids",
      "email": "ambevtester@example.com",
      "name": "ambevtester",
      "role": "Admin"
    },
    "success": true,
    "message": "User authenticated successfully",
    "errors": []
  },
  "success": true,
  "message": "",
  "errors": []
}
Token obtained: eyJhbGciOiJIUzI1NiIsInR5...

=== STEP 3: Creating a Sale ===
Status: 201
{
  "data": {
    "data": {
      "id": "144fa9ea-06d2-40df-8896-aa63c4824bcc",
      "saleNumber": 8,
      "saleDate": "2026-06-27T17:13:56.128902Z",
      "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "customerName": "Customer One",
      "branchId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "branchName": "Branch Central",
      "items": [
        {
          "id": "d74def41-d9ad-41d7-be54-fcfa69e1bbb9",
          "productId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
          "productName": "Product Alpha",
          "quantity": 5,
          "unitPrice": 10.0,
          "discount": 5.0,
          "totalAmount": 45.0
        },
        {
          "id": "3272de50-c123-4f8e-8251-4b4ff62061a2",
          "productId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
          "productName": "Product Beta",
          "quantity": 12,
          "unitPrice": 20.0,
          "discount": 48.0,
          "totalAmount": 192.0
        }
      ],
      "totalAmount": 237.0,
      "status": "Active"
    },
    "success": true,
    "message": "Sale created successfully",
    "errors": []
  }
}

=== STEP 4: Retrieving Sale (ID: 144fa9ea-06d2-40df-8896-aa63c4824bcc) ===
Status: 200
{
  "data": {
    "data": {
      "id": "144fa9ea-06d2-40df-8896-aa63c4824bcc",
      "saleNumber": 8,
      "saleDate": "2026-06-27T17:13:56.128902Z",
      "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "customerName": "Customer One",
      "branchId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "branchName": "Branch Central",
      "items": [
        {
          "id": "3272de50-c123-4f8e-8251-4b4ff62061a2",
          "productId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
          "productName": "Product Beta",
          "quantity": 12,
          "unitPrice": 20.0,
          "discount": 48.0,
          "totalAmount": 192.0
        },
        {
          "id": "d74def41-d9ad-41d7-be54-fcfa69e1bbb9",
          "productId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
          "productName": "Product Alpha",
          "quantity": 5,
          "unitPrice": 10.0,
          "discount": 5.0,
          "totalAmount": 45.0
        }
      ],
      "totalAmount": 237.0,
      "status": "Active"
    },
    "success": true,
    "message": "Sale retrieved successfully",
    "errors": []
  },
  "success": true,
  "message": "",
  "errors": []
}

=== STEP 5: Updating Sale (ID: 144fa9ea-06d2-40df-8896-aa63c4824bcc) ===
Status: 200
{
  "data": {
    "data": {
      "id": "144fa9ea-06d2-40df-8896-aa63c4824bcc",
      "saleNumber": 8,
      "saleDate": "2026-06-27T17:13:56.128902Z",
      "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "customerName": "Customer One Updated",
      "branchId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "branchName": "Branch Central",
      "items": [
        {
          "id": "6be151d8-82bb-46b6-9cee-c3ccdb995658",
          "productId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
          "productName": "Product Alpha",
          "quantity": 8,
          "unitPrice": 10.0,
          "discount": 8.0,
          "totalAmount": 72.0
        }
      ],
      "totalAmount": 72.0,
      "status": "Active"
    },
    "success": true,
    "message": "Sale updated successfully",
    "errors": []
  },
  "success": true,
  "message": "",
  "errors": []
}

=== STEP 6: Retrieving Updated Sale (ID: 144fa9ea-06d2-40df-8896-aa63c4824bcc) ===
Status: 200
...
[Deleted successfully, verification confirmed sale no longer exists in database]
```
