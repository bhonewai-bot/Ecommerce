-- Seed data for MVP catalog (categories + products)

INSERT INTO categories (id, name, description, delete_flag) VALUES
  (1, 'Electronics', 'Gadgets and devices', FALSE),
  (2, 'Home & Kitchen', 'Essentials for home and cooking', FALSE),
  (3, 'Books', 'Fiction and non-fiction reads', FALSE),
  (4, 'Fashion', 'Clothing and accessories', FALSE),
  (5, 'Sports & Outdoors', 'Gear for fitness and outdoor activities', FALSE),
  (6, 'Beauty & Personal Care', 'Skincare and grooming', FALSE)
ON CONFLICT (id) DO NOTHING;

SELECT setval(
               pg_get_serial_sequence('categories','id'),
               (SELECT COALESCE(MAX(id), 1) FROM categories),
               true
       );

INSERT INTO products (category_id, name, description, price, image_url, delete_flag) VALUES
  (1, 'Wireless Earbuds', 'Compact earbuds with charging case', 49.99, '/images/products/earbuds.jpg', FALSE),
  (1, 'Bluetooth Speaker', 'Portable speaker with deep bass', 39.50, '/images/products/speaker.jpg', FALSE),
  (1, 'Smart Watch', 'Fitness tracking and notifications', 79.00, '/images/products/smartwatch.jpg', FALSE),
  (1, 'USB-C Charger', 'Fast charging 30W adapter', 19.99, '/images/products/charger.jpg', FALSE),
  (1, 'Noise-Canceling Headphones', 'Over-ear ANC headphones', 129.99, '/images/products/headphones.jpg', FALSE),
  (1, 'Portable SSD 1TB', 'High-speed external storage', 89.99, '/images/products/ssd.jpg', FALSE),
  (1, '1080p Webcam', 'HD webcam for calls and streaming', 34.95, '/images/products/webcam.jpg', FALSE),
  (1, 'Mechanical Keyboard', 'Tactile switches, RGB backlight', 64.99, '/images/products/keyboard.jpg', FALSE),
  (1, 'Wireless Mouse', 'Ergonomic mouse with DPI control', 24.50, '/images/products/mouse.jpg', FALSE),

  (2, 'Stainless Cookware Set', '10-piece pots and pans', 149.00, '/images/products/cookware.jpg', FALSE),
  (2, 'Air Fryer 4L', 'Crispy cooking with less oil', 89.00, '/images/products/airfryer.jpg', FALSE),
  (2, 'Vacuum Cleaner', 'Bagless vacuum with HEPA filter', 119.99, '/images/products/vacuum.jpg', FALSE),
  (2, 'Memory Foam Pillow', 'Cooling and ergonomic support', 29.99, '/images/products/pillow.jpg', FALSE),
  (2, 'Ceramic Dinner Set', '12-piece dinnerware set', 54.00, '/images/products/dinnerware.jpg', FALSE),
  (2, 'Electric Kettle', '1.7L fast-boil kettle', 28.75, '/images/products/kettle.jpg', FALSE),
  (2, 'Glass Food Containers', 'Set of 10 storage containers', 32.50, '/images/products/containers.jpg', FALSE),
  (2, 'Blender 700W', 'Smoothies and sauces', 45.99, '/images/products/blender.jpg', FALSE),

  (3, 'Modern Web Development', 'Guide to web apps and APIs', 24.99, '/images/products/book-web.jpg', FALSE),
  (3, 'Clean Architecture', 'Software design principles', 29.99, '/images/products/book-arch.jpg', FALSE),
  (3, 'Practical PostgreSQL', 'Database fundamentals and tuning', 27.50, '/images/products/book-pg.jpg', FALSE),
  (3, 'Productivity Habits', 'Small habits for big results', 18.00, '/images/products/book-habits.jpg', FALSE),
  (3, 'Intro to Machine Learning', 'Foundations and applications', 31.25, '/images/products/book-ml.jpg', FALSE),
  (3, 'Cooking Basics', 'Everyday recipes and techniques', 21.00, '/images/products/book-cook.jpg', FALSE),
  (3, 'Mindful Living', 'Simple practices for calm', 16.95, '/images/products/book-mindful.jpg', FALSE),

  (4, 'Cotton T-Shirt', 'Soft everyday tee', 12.99, '/images/products/tshirt.jpg', FALSE),
  (4, 'Slim Fit Jeans', 'Stretch denim, dark wash', 39.99, '/images/products/jeans.jpg', FALSE),
  (4, 'Running Sneakers', 'Lightweight and breathable', 59.99, '/images/products/sneakers.jpg', FALSE),
  (4, 'Classic Hoodie', 'Fleece-lined zip hoodie', 34.50, '/images/products/hoodie.jpg', FALSE),
  (4, 'Leather Belt', 'Genuine leather, adjustable', 19.50, '/images/products/belt.jpg', FALSE),
  (4, 'Wool Beanie', 'Warm knit beanie', 14.00, '/images/products/beanie.jpg', FALSE),
  (4, 'Summer Dress', 'Casual midi dress', 44.00, '/images/products/dress.jpg', FALSE),
  (4, 'Sports Jacket', 'Wind-resistant lightweight jacket', 69.00, '/images/products/jacket.jpg', FALSE),

  (5, 'Yoga Mat', 'Non-slip 6mm mat', 22.99, '/images/products/yogamat.jpg', FALSE),
  (5, 'Dumbbell Set', 'Adjustable weights', 74.99, '/images/products/dumbbells.jpg', FALSE),
  (5, 'Insulated Water Bottle', '24oz stainless bottle', 18.50, '/images/products/bottle.jpg', FALSE),
  (5, 'Camping Tent 2P', 'Lightweight 2-person tent', 129.00, '/images/products/tent.jpg', FALSE),
  (5, 'Hiking Backpack', '30L daypack with support', 52.00, '/images/products/backpack.jpg', FALSE),
  (5, 'Resistance Bands', 'Set of 5 bands', 16.99, '/images/products/bands.jpg', FALSE),
  (5, 'Jump Rope', 'Adjustable speed rope', 9.99, '/images/products/jumprope.jpg', FALSE),
  (5, 'Bike Helmet', 'Lightweight protective helmet', 36.00, '/images/products/helmet.jpg', FALSE),

  (6, 'Facial Cleanser', 'Gentle daily cleanser', 11.99, '/images/products/cleanser.jpg', FALSE),
  (6, 'Moisturizing Lotion', 'Hydrating body lotion', 13.50, '/images/products/lotion.jpg', FALSE),
  (6, 'Sunscreen SPF50', 'Broad-spectrum protection', 15.99, '/images/products/sunscreen.jpg', FALSE),
  (6, 'Shampoo & Conditioner', 'Strengthening care duo', 19.00, '/images/products/shampoo.jpg', FALSE),
  (6, 'Beard Trimmer', 'Rechargeable grooming kit', 27.99, '/images/products/trimmer.jpg', FALSE),
  (6, 'Lip Balm', 'Shea butter lip care', 3.99, '/images/products/lipbalm.jpg', FALSE),
  (6, 'Hand Cream', 'Repairing hand moisturizer', 7.50, '/images/products/handcream.jpg', FALSE),
  (6, 'Bath Set', 'Soap and bath essentials', 24.00, '/images/products/bathset.jpg', FALSE),
  (6, 'Hair Dryer', 'Ionic fast-dry dryer', 39.00, '/images/products/hairdryer.jpg', FALSE);
