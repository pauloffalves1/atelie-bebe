export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  price: number;
  category: string;
  imageUrl: string | null;
  active: boolean;
  featured: boolean;
  isExclusive: boolean;
}

export interface AdminProduct extends Product {
  allowedCustomerIds: string[];
}

export interface CreateProductRequest {
  name: string;
  description: string | null;
  price: number;
  category: string;
  imageUrl: string | null;
  featured: boolean;
}

export interface UpdateProductRequest {
  name: string;
  description: string | null;
  price: number;
  category: string;
  imageUrl: string | null;
  featured: boolean;
}
