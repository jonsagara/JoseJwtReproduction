# Original Issue

Here is the issue in the jose-jwt repo:

https://github.com/dvsekhvalnov/jose-jwt/issues/229


# Workaround

The library author [suggested a few workarounds](https://github.com/dvsekhvalnov/jose-jwt/issues/229#issuecomment-1629114960). Option #3 worked best for me:

- Decode to string
- Deserialize the string to the destination model
 
I updated this reproduction to demonstrate the workaround.