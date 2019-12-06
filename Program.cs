  using furb_cg_trabalho_final_meinkraft;
  
  class Program
  {
    static void Main(string[] args)
    {
      Mundo window = Mundo.GetInstance(1200, 1200);
      window.Title = "Meinkraft";
      window.Run(1.0 / 60.0);
    }
  }