$API = "http://localhost:5098/api"

function Login([string]$email, [string]$pass) {
    $body = '{"email":"' + $email + '","password":"' + $pass + '"}'
    $res = Invoke-RestMethod -Uri "$API/Auth/login" -Method POST -Body $body -ContentType "application/json"
    return $res.token
}

function Register([string]$name, [string]$email, [string]$pass) {
    try {
        $body = '{"name":"' + $name + '","email":"' + $email + '","password":"' + $pass + '"}'
        $res = Invoke-RestMethod -Uri "$API/Auth/register" -Method POST -Body $body -ContentType "application/json"
        Write-Host "  Registered: $name" -ForegroundColor Green
        return $res.token
    } catch {
        Write-Host "  Already exists, logging in: $name" -ForegroundColor Yellow
        return Login $email $pass
    }
}

function CreateCategory([string]$token, [string]$title, [string]$icon, [string]$type) {
    try {
        $headers = @{ Authorization = "Bearer $token" }
        $body = '{"title":"' + $title + '","icon":"' + $icon + '","type":"' + $type + '"}'
        $res = Invoke-RestMethod -Uri "$API/Category" -Method POST -Headers $headers -Body $body -ContentType "application/json"
        return $res.categoryId
    } catch { return $null }
}

function GetCategories([string]$token) {
    $headers = @{ Authorization = "Bearer $token" }
    return Invoke-RestMethod -Uri "$API/Category" -Method GET -Headers $headers
}

function AddExpense([string]$token, [int]$catId, [int]$amount, [string]$note, [int]$daysAgo) {
    try {
        $headers = @{ Authorization = "Bearer $token" }
        $date = (Get-Date).AddDays(-$daysAgo).ToString("yyyy-MM-ddTHH:mm:ss")
        $body = '{"categoryId":' + $catId + ',"amount":' + $amount + ',"note":"' + $note + '","date":"' + $date + '","isRecurring":false}'
        Invoke-RestMethod -Uri "$API/Transaction" -Method POST -Headers $headers -Body $body -ContentType "application/json" | Out-Null
        Write-Host "    + Rs.$amount - $note" -ForegroundColor Cyan
    } catch {
        Write-Host "    ! Failed: $note - $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== RupeeFlow Demo Data Seeder ===" -ForegroundColor Magenta
Write-Host ""

# ------------------ USER 1: Meet (existing) ------------------
Write-Host "[1] Seeding for Meet (meet@gmail.com)..." -ForegroundColor White
$t1 = Login "meet@gmail.com" "Meet2701"

$cats1   = GetCategories $t1
$food1   = ($cats1 | Where-Object { $_.title -like "*Food*"          } | Select-Object -First 1).categoryId
$trans1  = ($cats1 | Where-Object { $_.title -like "*Transport*"     } | Select-Object -First 1).categoryId
$shop1   = ($cats1 | Where-Object { $_.title -like "*Shopping*"      } | Select-Object -First 1).categoryId
$util1   = ($cats1 | Where-Object { $_.title -like "*Utilities*"     } | Select-Object -First 1).categoryId
$ent1    = ($cats1 | Where-Object { $_.title -like "*Entertainment*" } | Select-Object -First 1).categoryId
$health1 = ($cats1 | Where-Object { $_.title -like "*Health*"        } | Select-Object -First 1).categoryId

if (-not $food1) {
    Write-Host "  Creating categories..." -ForegroundColor Yellow
    $food1   = CreateCategory $t1 "Food and Dining" "F" "Expense"
    $trans1  = CreateCategory $t1 "Transport"        "T" "Expense"
    $shop1   = CreateCategory $t1 "Shopping"         "S" "Expense"
    $util1   = CreateCategory $t1 "Utilities"        "U" "Expense"
    $ent1    = CreateCategory $t1 "Entertainment"    "E" "Expense"
    $health1 = CreateCategory $t1 "Health"           "H" "Expense"
}

Write-Host "  Adding expenses for Meet..." -ForegroundColor White
AddExpense $t1 $food1   320  "Zomato order Biryani"       0
AddExpense $t1 $trans1  150  "Ola cab to office"           1
AddExpense $t1 $food1   85   "Chai and samosa"             1
AddExpense $t1 $shop1   2499 "Amazon USB-C hub"            2
AddExpense $t1 $util1   899  "Jio 3-month recharge"        3
AddExpense $t1 $food1   450  "Dominos pizza night"         3
AddExpense $t1 $trans1  60   "Metro card top-up"           4
AddExpense $t1 $ent1    649  "Netflix subscription"        5
AddExpense $t1 $food1   180  "Breakfast at coffee day"     6
AddExpense $t1 $health1 1200 "Apollo pharmacy"             7
AddExpense $t1 $shop1   599  "Myntra T-shirts"             8
AddExpense $t1 $food1   240  "Swiggy Punjabi thali"        8
AddExpense $t1 $trans1  200  "Petrol 2 litres"             9
AddExpense $t1 $util1   750  "Electricity bill"            10
AddExpense $t1 $ent1    299  "Hotstar subscription"        11
AddExpense $t1 $food1   95   "Canteen lunch"               12
AddExpense $t1 $trans1  45   "Auto to market"              13
AddExpense $t1 $shop1   1299 "Puma sports socks"           14
AddExpense $t1 $food1   380  "Birthday dinner Pizza Hut"   15
AddExpense $t1 $health1 450  "Gym monthly fee"             16
AddExpense $t1 $util1   199  "Google One storage"          17
AddExpense $t1 $food1   110  "Street food Pav Bhaji"       18
AddExpense $t1 $trans1  350  "Uber ride airport drop"      20
AddExpense $t1 $shop1   849  "Boat earphones"              22
AddExpense $t1 $food1   275  "Starbucks cold brew"         25
AddExpense $t1 $ent1    199  "Amazon Prime monthly"        27
AddExpense $t1 $health1 300  "Chemist vitamins"            28
AddExpense $t1 $food1   490  "Dinner family restaurant"    30
AddExpense $t1 $trans1  80   "Local bus pass"              32
AddExpense $t1 $shop1   3199 "Wildcraft backpack"          35

Write-Host "  Done for Meet." -ForegroundColor Green

# ------------------ USER 2: Priya Sharma ------------------
Write-Host ""
Write-Host "[2] Seeding user Priya Sharma (priya@demo.com)..." -ForegroundColor White
$t2 = Register "Priya Sharma" "priya@demo.com" "Priya1234"

$food2   = CreateCategory $t2 "Food and Dining" "F" "Expense"
$trans2  = CreateCategory $t2 "Transport"        "T" "Expense"
$shop2   = CreateCategory $t2 "Shopping"         "S" "Expense"
$util2   = CreateCategory $t2 "Utilities"        "U" "Expense"
$edu2    = CreateCategory $t2 "Education"        "E" "Expense"
$health2 = CreateCategory $t2 "Health"           "H" "Expense"

Write-Host "  Adding expenses for Priya..." -ForegroundColor White
AddExpense $t2 $food2   120  "College canteen lunch"       0
AddExpense $t2 $trans2  40   "Auto rickshaw to college"    0
AddExpense $t2 $edu2    599  "Udemy Python course"         1
AddExpense $t2 $food2   75   "Maggi and juice"             2
AddExpense $t2 $util2   299  "Jio 28-day recharge"         3
AddExpense $t2 $shop2   899  "Stationery and notebooks"    4
AddExpense $t2 $food2   200  "Dominos Friday special"      5
AddExpense $t2 $trans2  120  "Ola to market"               6
AddExpense $t2 $edu2    1499 "Programming book Flipkart"   7
AddExpense $t2 $health2 350  "Doctor consultation"         8
AddExpense $t2 $food2   90   "Breakfast idli sambar"       9
AddExpense $t2 $trans2  60   "Metro card"                  10
AddExpense $t2 $shop2   499  "Hair care products"          12
AddExpense $t2 $food2   310  "Team dinner contribution"    14
AddExpense $t2 $edu2    299  "Coursera 1-month"            16
AddExpense $t2 $util2   149  "Spotify premium"             18
AddExpense $t2 $food2   180  "Evening snacks samosa chaat" 20
AddExpense $t2 $trans2  200  "Bus pass monthly"            22
AddExpense $t2 $health2 199  "Pharmacy cold medicines"     25
AddExpense $t2 $food2   450  "Birthday treat ice cream"    28

Write-Host "  Done for Priya." -ForegroundColor Green

# ------------------ USER 3: Arjun Verma ------------------
Write-Host ""
Write-Host "[3] Seeding user Arjun Verma (arjun@demo.com)..." -ForegroundColor White
$t3 = Register "Arjun Verma" "arjun@demo.com" "Arjun5678"

$food3   = CreateCategory $t3 "Food and Dining" "F" "Expense"
$trans3  = CreateCategory $t3 "Transport"        "T" "Expense"
$shop3   = CreateCategory $t3 "Shopping"         "S" "Expense"
$util3   = CreateCategory $t3 "Utilities"        "U" "Expense"
$ent3    = CreateCategory $t3 "Entertainment"    "E" "Expense"
$travel3 = CreateCategory $t3 "Travel"           "V" "Expense"

Write-Host "  Adding expenses for Arjun..." -ForegroundColor White
AddExpense $t3 $food3   850  "Swiggy office dinner"        0
AddExpense $t3 $trans3  600  "Ola monthly pass"            1
AddExpense $t3 $ent3    649  "Netflix and Hotstar"         2
AddExpense $t3 $food3   1200 "Weekend brunch"              3
AddExpense $t3 $shop3   4999 "Samsung wireless charger"    4
AddExpense $t3 $util3   1200 "Electricity bill"            5
AddExpense $t3 $travel3 8500 "Goa trip flight"             6
AddExpense $t3 $food3   2400 "Goa trip meals"              6
AddExpense $t3 $trans3  350  "Rapido bike cab"             8
AddExpense $t3 $shop3   2299 "Levis jeans"                 10
AddExpense $t3 $food3   480  "Tea and snacks office"       12
AddExpense $t3 $ent3    199  "BookMyShow movie tickets"    14
AddExpense $t3 $util3   599  "Google One 100GB"            15
AddExpense $t3 $food3   950  "Family dinner Punjab Grill"  18
AddExpense $t3 $trans3  250  "Parking and petrol"          20
AddExpense $t3 $shop3   1899 "JBL earphones"               22
AddExpense $t3 $travel3 2200 "Mumbai trip train tickets"   25
AddExpense $t3 $food3   660  "Zomato weekend special"      28
AddExpense $t3 $ent3    399  "SonyLiv annual plan"         30
AddExpense $t3 $food3   1100 "Team outing lunch"           33

Write-Host "  Done for Arjun." -ForegroundColor Green

Write-Host ""
Write-Host "=== Seeding Complete! ===" -ForegroundColor Magenta
Write-Host ""
Write-Host "Demo Users:" -ForegroundColor White
Write-Host "  1. meet@gmail.com  / Meet2701   (Your account - 30+ expenses)" -ForegroundColor Cyan
Write-Host "  2. priya@demo.com  / Priya1234  (Student user - 20 expenses)"  -ForegroundColor Cyan
Write-Host "  3. arjun@demo.com  / Arjun5678  (Professional - 20 expenses)"  -ForegroundColor Cyan
Write-Host ""
Write-Host "Open http://localhost:4200 to see the app." -ForegroundColor Green
