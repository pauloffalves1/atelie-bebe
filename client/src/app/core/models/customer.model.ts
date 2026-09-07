export interface CustomerSummary {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  cpf: string | null;
  createdAt: string;
  isAnonymized: boolean;
}

export interface UpdateCustomerRequest {
  name: string;
  email: string;
  cpf: string;
  phone: string | null;
}
