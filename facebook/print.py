# This program adds two numbers

num1 = 1.5
num2 = 6.3

# Add two numbers
sum = float(num1) + float(num2)
suma = float(sum) + float(num1)
sumb = float(sum) + float(num2)

# Display the sum
print('The sum of {0} and {1} is {2}'.format(num1, num2, sum))
print('The sum of {0} and {1} is {2}'.format(sum, num1, suma))
print('The sum of {0} and {1} is {2}'.format(sum, num2, sumb))