#include <stdio.h>

int altura,i,j;
float x,y;

void main()
{
    printf("\nValor de altura = ");
    scanf("",&altura);

    for (i = 1; i<=altura; i++)
    {
        for (j = 1; j<=i; j++)
        {
            if (j%2==0)
			{
				printf("*");
			}
			else
			{
				printf("-");
			}
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
   /* for (i = altura; i>=0; i--)
    {
        
        j = 1;
        while (j<=i)
        { 
			if (j%2==0)
			{
				printf("*");
			}
			else
			{
				printf("-");
			}
            j++;
        }
        printf("\n");
    }
    */
    i = 0;
    do
    {
        printf("-");
        i++;
    }
    while (i<altura*2);
    printf("\n");
}

