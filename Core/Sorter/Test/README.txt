# Run tests after changing and before pushing.


## How to run tests

Simply call the "Start" method in TestAll.cs.

To do this, I create an instance of TestAll in Core/CoreAlgorithm.cs and call Start();

Do not create an instance in any of the tested classes, this may cause a stack overflow exception.


## If they succeed

If you run the tests and it says:

"!!! FINAL RESULT !!!
:::Tests Passed:::"

Then it has succeeded! You may attempt a push.


## If they fail

1. Your code breaks something

Ensure that your code is not breaking any other code.

2. The tests are flawed

If you find a flaw in the tests, you may report and try fixing them.