  using furb_cg_trabalho_final_meinkraft;
  
  class Program
  {
    static void Main(string[] args)
    {
      Mundo window = Mundo.GetInstance(600, 600);
      window.Title = "Meinkraft";
      window.Run(1.0 / 60.0);
    }
  }