export interface CustomerSummary {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  cpf: string | null;
  createdAt: string;
}

export interface UpdateCustomerRequest {
  name: string;
  email: string;
  cpf: string;
  phone: string | null;
}
