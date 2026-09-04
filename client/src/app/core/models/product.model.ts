export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  price: number;
  category: string;
  imageUrl: string | null;
  stock: number;
  active: boolean;
  featured: boolean;
}

export interface CreateProductRequest {
  name: string;
  description: string | null;
  price: number;
  category: string;
  imageUrl: string | null;
  stock: number;
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
