import os 
for i in range(36,43):
    for j in range(26,46):
        os.system('copy dummy.asset ' + str(i)+'N0'+str(j)+'E.asset')