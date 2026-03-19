import { Component, OnInit, Renderer2 } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartType } from 'chart.js';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  apiUrl = 'http://localhost:5098/api';
  token = '';
  isLoggedIn = false;
  isLoading = false;
  errorMsg = '';

  // Auth — login with name + password
  loginName = '';
  loginPassword = '';

  // Register fields
  regName = '';
  regEmail = '';
  regPassword = '';

  // User info
  userName = '';
  authMode: 'login' | 'register' = 'login';

  // Theme
  isDarkMode = false;

  // Expenses
  allTransactions: any[] = [];
  filteredTransactions: any[] = [];
  categories: any[] = [];     // only Expense categories

  // Add expense form
  newAmount: number = 0;
  newCategoryId: number = 0;
  newNote: string = '';
  newDate: string = new Date().toISOString().split('T')[0];

  // ------ FILTERS ------
  filterCategory: number = 0;          // 0 = All
  filterMonth: string = '';             // "2026-03"
  filterDateFrom: string = '';
  filterDateTo: string = '';
  filterNote: string = '';
  filterSort: 'date_desc' | 'date_asc' | 'amount_desc' | 'amount_asc' = 'date_desc';

  // Summary for filtered view
  get filteredTotal(): number {
    return this.filteredTransactions.reduce((s, t) => s + t.amount, 0);
  }

  get filteredCount(): number {
    return this.filteredTransactions.length;
  }

  // Chart
  public barChartType: ChartType = 'bar';
  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (ctx: any) => `₹${ctx.parsed.y.toLocaleString('en-IN')}`
        }
      }
    },
    scales: {
      y: {
        ticks: {
          callback: (v: any) => `₹${Number(v).toLocaleString('en-IN')}`
        },
        grid: { color: '#f0f0f0' }
      },
      x: { grid: { display: false } }
    }
  };
  public barChartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [{ data: [], label: 'Expense', backgroundColor: '#e55a4e', borderRadius: 4 }]
  };

  constructor(private http: HttpClient, private renderer: Renderer2) { }

  ngOnInit() {
    this.token = localStorage.getItem('token') || '';
    this.userName = localStorage.getItem('userName') || '';
    // Restore theme
    const savedTheme = localStorage.getItem('theme') || 'light';
    this.isDarkMode = savedTheme === 'dark';
    this.applyTheme();
    if (this.token) {
      this.isLoggedIn = true;
      this.loadCategories();
      this.loadTransactions();
    }
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
    this.applyTheme();
  }

  private applyTheme() {
    if (this.isDarkMode) {
      this.renderer.addClass(document.body, 'dark');
    } else {
      this.renderer.removeClass(document.body, 'dark');
    }

    const textColor = this.isDarkMode ? '#aaa' : '#666';
    const gridColor = this.isDarkMode ? '#333' : '#f0f0f0';
    this.barChartOptions = {
      ...this.barChartOptions,
      scales: {
        y: {
          ticks: {
            callback: (v: any) => `₹${Number(v).toLocaleString('en-IN')}`,
            color: textColor
          },
          grid: { color: gridColor }
        },
        x: {
          ticks: { color: textColor },
          grid: { display: false }
        }
      }
    };
  }

  getHeaders() {
    return new HttpHeaders().set('Authorization', `Bearer ${this.token}`);
  }

  // ---- AUTH ----
  login() {
    if (!this.loginName.trim() || !this.loginPassword.trim()) {
      this.errorMsg = 'Please enter your name and password.';
      return;
    }
    this.isLoading = true;
    this.errorMsg = '';
    // The API uses email for auth, but we stored name; we'll match by searching users or use a name-based workaround.
    // We look up the email by using the name as a unique identifier fallback via email field trick.
    // Since the backend needs email, we'll accept email OR name in login field.
    this.http.post(`${this.apiUrl}/Auth/login`, { email: this.loginName, password: this.loginPassword })
      .subscribe({
        next: (res: any) => {
          this.token = res.token;
          this.userName = res.user.name;
          localStorage.setItem('token', this.token);
          localStorage.setItem('userName', this.userName);
          this.isLoggedIn = true;
          this.isLoading = false;
          this.loadCategories();
          this.loadTransactions();
        },
        error: () => {
          this.isLoading = false;
          this.errorMsg = 'Incorrect credentials. Please try again.';
        }
      });
  }

  register() {
    if (!this.regName.trim() || !this.regEmail.trim() || !this.regPassword.trim()) {
      this.errorMsg = 'Please fill all fields.';
      return;
    }
    this.isLoading = true;
    this.errorMsg = '';
    this.http.post(`${this.apiUrl}/Auth/register`, {
      name: this.regName,
      email: this.regEmail,
      password: this.regPassword
    }).subscribe({
      next: (res: any) => {
        this.token = res.token;
        this.userName = res.user.name;
        localStorage.setItem('token', this.token);
        localStorage.setItem('userName', this.userName);
        this.isLoggedIn = true;
        this.isLoading = false;
        this.createDefaultExpenseCategories();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMsg = err.error?.message || 'Registration failed.';
      }
    });
  }

  logout() {
    this.token = '';
    this.userName = '';
    localStorage.clear();
    this.isLoggedIn = false;
    this.allTransactions = [];
    this.filteredTransactions = [];
  }

  // ---- DATA ----
  loadCategories() {
    this.http.get<any[]>(`${this.apiUrl}/Category`, { headers: this.getHeaders() })
      .subscribe(res => {
        // Only show Expense categories
        this.categories = res.filter(c => c.type === 'Expense');
        if (this.categories.length > 0 && this.newCategoryId === 0) {
          this.newCategoryId = this.categories[0].categoryId;
        }
      });
  }

  loadTransactions() {
    this.http.get<any[]>(`${this.apiUrl}/Transaction`, { headers: this.getHeaders() })
      .subscribe(res => {
        // Only keep Expense type (filter out Income)
        this.allTransactions = res
          .filter(t => t.category?.type === 'Expense')
          .map(t => ({ ...t, date: new Date(t.date) }));
        this.applyFilters();
        this.buildChart();
      });
  }

  createDefaultExpenseCategories() {
    const cats = [
      { title: 'Food & Dining', icon: '🍱', type: 'Expense' },
      { title: 'Transport', icon: '🚌', type: 'Expense' },
      { title: 'Shopping', icon: '🛍️', type: 'Expense' },
      { title: 'Utilities', icon: '💡', type: 'Expense' },
      { title: 'Entertainment', icon: '🎮', type: 'Expense' },
      { title: 'Health', icon: '💊', type: 'Expense' },
    ];
    const next = (i: number) => {
      if (i >= cats.length) { this.loadCategories(); this.loadTransactions(); return; }
      this.http.post(`${this.apiUrl}/Category`, cats[i], { headers: this.getHeaders() })
        .subscribe(() => next(i + 1));
    };
    next(0);
  }

  addExpense() {
    if (!this.newAmount || this.newAmount <= 0) { this.errorMsg = 'Enter a valid amount.'; return; }
    if (!this.newCategoryId) { this.errorMsg = 'Select a category.'; return; }
    this.errorMsg = '';

    const body = {
      categoryId: this.newCategoryId,
      amount: this.newAmount,
      note: this.newNote,
      date: new Date(this.newDate).toISOString(),
      isRecurring: false
    };

    this.http.post(`${this.apiUrl}/Transaction`, body, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          this.newAmount = 0;
          this.newNote = '';
          this.loadTransactions();
        },
        error: () => { this.errorMsg = 'Failed to add expense. Check category.'; }
      });
  }

  deleteExpense(id: number) {
    this.http.delete(`${this.apiUrl}/Transaction/${id}`, { headers: this.getHeaders() })
      .subscribe(() => this.loadTransactions());
  }

  // ---- FILTERS ----
  applyFilters() {
    let result = [...this.allTransactions];

    if (this.filterCategory && this.filterCategory !== 0) {
      result = result.filter(t => t.categoryId === +this.filterCategory);
    }

    if (this.filterMonth) {
      const [yr, mo] = this.filterMonth.split('-').map(Number);
      result = result.filter(t => {
        const d = new Date(t.date);
        return d.getFullYear() === yr && d.getMonth() + 1 === mo;
      });
    }

    if (this.filterDateFrom) {
      const from = new Date(this.filterDateFrom);
      result = result.filter(t => new Date(t.date) >= from);
    }

    if (this.filterDateTo) {
      const to = new Date(this.filterDateTo);
      to.setHours(23, 59, 59);
      result = result.filter(t => new Date(t.date) <= to);
    }

    if (this.filterNote.trim()) {
      const q = this.filterNote.toLowerCase();
      result = result.filter(t => t.note?.toLowerCase().includes(q));
    }

    // Sort
    result.sort((a, b) => {
      switch (this.filterSort) {
        case 'date_asc': return new Date(a.date).getTime() - new Date(b.date).getTime();
        case 'amount_desc': return b.amount - a.amount;
        case 'amount_asc': return a.amount - b.amount;
        default: return new Date(b.date).getTime() - new Date(a.date).getTime();
      }
    });

    this.filteredTransactions = result;
  }

  clearFilters() {
    this.filterCategory = 0;
    this.filterMonth = '';
    this.filterDateFrom = '';
    this.filterDateTo = '';
    this.filterNote = '';
    this.filterSort = 'date_desc';
    this.applyFilters();
  }

  // ---- CHART ----
  buildChart() {
    // Group spending by category
    const map = new Map<string, number>();
    for (const t of this.allTransactions) {
      const key = (t.category?.icon ?? '') + ' ' + (t.category?.title ?? 'Other');
      map.set(key, (map.get(key) ?? 0) + t.amount);
    }

    const sorted = [...map.entries()].sort((a, b) => b[1] - a[1]);
    this.barChartData = {
      labels: sorted.map(e => e[0]),
      datasets: [{
        data: sorted.map(e => e[1]),
        label: 'Expense',
        backgroundColor: ['#e55a4e', '#f4976c', '#f7c59f', '#eddea4', '#b5c7c9', '#9a8c98'],
        borderRadius: 5
      }]
    };
  }

  formatDate(d: any): string {
    return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  getCategoryName(t: any): string {
    return (t.category?.icon ?? '') + ' ' + (t.category?.title ?? '—');
  }

  // Month label for filter
  months = [
    { value: new Date().toISOString().slice(0, 7), label: 'This Month' },
    ...[1, 2, 3, 4, 5].map(i => {
      const d = new Date();
      d.setMonth(d.getMonth() - i);
      const v = d.toISOString().slice(0, 7);
      return { value: v, label: d.toLocaleString('en-IN', { month: 'long', year: 'numeric' }) };
    })
  ];
}
