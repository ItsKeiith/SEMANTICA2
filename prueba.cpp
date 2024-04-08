#include <stdio.h>

int altura,i,j;
float x,y;

void main()
{
    y = 10;
    x = (3 + 5) * 8 - (10 - 4) / 2; // = 61
    x--;
    x+=40;
    x*=2;
    x/=(y-6);
    printf("\nValor de altura = ");
    scanf("",&altura);

    for (i = 1; i<=altura; i++)
    {
        for (j = 1; j<=i; j++)
        {
            printf("%f", j);
        }
        printf("\n");
    }
    i = 0;
    do
    {
        printf("-");
        i++;
    }
    while (i<altura*2);
    printf("\n");
    for (i = 1; i<=altura; i++)
    {
        j = 1;
        while (j<=i)
        { 
            printf("%f", j);
            j++;
        }
        printf("\n");
    }
    i = 0;
    do
    {
        printf("-");
        i++;
    }
    while (i<altura*2);
    printf("\n");
}