# Sensitive Data Compliance & Protection

## Overview

This guide details how the Retail Core handles **Sensitive Data Compliance**, specifically addressing the protection of Customer Personally Identifiable Information (PII) and Financial Data. The system is designed to adhere to strict regulatory standards, including the **FTC Safeguards Rule (16 CFR Part 314)**.

## Compliance Guarantees

The architecture provides two fundamental guarantees for any data marked as `[SensitiveData]`:

### 1. Encryption at Rest (Data Confidentiality)
**Requirement:** Customer financial and personal data must be unreadable if physical storage is compromised.
**Solution:**
- **Column-Level Encryption:** Data is encrypted *before* it leaves the application memory. The database stores only AES-256-GCM ciphertext.
- **Defense in Depth:** Even a database administrator with full access cannot read customer SSNs, account numbers, or internal costs without the specific application encryption key.

### 2. Forensic Reconstructability (Audit Integrity)
**Requirement:** You must be able to reconstruct exactly *what* changed and *who* changed it, even if the data is sensitive.
**Solution:**
- **Ciphertext Auditing:** The audit log stores the **Encrypted Ciphertext** of the *old* and *new* values.
- **Why this matters:** Unlike "masking" (which destroys history like `***`), storing ciphertext allows an authorized Security Officer (possessing the key) to decrypt the audit log and reconstruct a breach timeline, satisfying forensic requirements while keeping the logs safe from casual review.

## Implementation Guide

### Protecting Customer Data

To bring a property under compliance protection, annotate it with the `[SensitiveData]` attribute. The system handles all encryption, decryption, and logging automatically.

#### Example: Financial Entity
```csharp
public class CustomerFinancialProfile : IAuditableEntity
{
    public Guid Id { get; private set; }

    // Standard data (Visible in DB/Logs)
    public string AccountStatus { get; private set; }

    // COMPLIANT DATA (Encrypted in DB + Logs)
    [SensitiveData]
    public string BankAccountNumber { get; private set; }

    [SensitiveData]
    public decimal CreditLimit { get; private set; }
}
```

### Supported Data Types
The compliance engine supports:
- **Strings:** Direct encryption (e.g., PII, Notes).
- **Complex Types:** Automatic JSON serialization + encryption (e.g., `decimal`, `Money` value objects, nested DTOs).

## Key Management & Operations

**Dev vs. Prod:**
- **Development:** Uses a preset key for instant setup ("F5 Ready").
  ```json
  "Encryption": {
    "Key": "MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI="
  }
  ```
- **Production:** Requires injecting `Encryption:Key` via secure environment variables or Secret Managers (AWS Secrets Manager).

### Key Rotation Strategy (Future-Proofing)

The current implementation uses a **Single Active Key**, but all ciphertext is prefixed with a Key ID (e.g., `1:Ai8...`). This allows for seamless key rotation in the future without re-encrypting the entire database.

**How to Rotate Keys:**
1.  **Generate a New Key** (e.g., Key ID "2").
2.  **Update Configuration** to provide *both* keys (Active = "2", Passive = "1").
3.  **Update `AesEncryptionService`** to select the correct decryption key based on the prefix:

```csharp
// Concept for Future Multi-Key Service
public string Decrypt(string cipherText)
{
    var (keyId, payload) = ParsePrefix(cipherText); // e.g., returns "1"
    
    // Look up the old key from configuration/KeyRing
    var key = _keyRing[keyId]; 
    
    return Decrypt(payload, key);
}
```

**Adding New Sensitive Data:**
To protect a new column, **no crypto setup is required**. Simply add the `[SensitiveData]` attribute. The system will automatically use the configured master key.

---
*Regulatory Reference: [Code of Federal Regulations, Title 16, Part 314](https://www.ecfr.gov/current/title-16/chapter-I/subchapter-C/part-314)*
