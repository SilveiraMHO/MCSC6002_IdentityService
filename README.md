# MCSC6002_IdentityService
Service responsible for centralizing user sign-in.

## JWT signing keys (RS256)
Tokens are signed with an RSA private key that is never committed. Generate the pair once in a `keys/` folder next to the service repos:

```bash
mkdir keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/jwt-private.pem
openssl pkey -in keys/jwt-private.pem -pubout -out keys/jwt-public.pem
```

Only this service gets `jwt-private.pem` (`TokenSettings:PrivateKeyPath`). The gateway and other services get `jwt-public.pem`.
